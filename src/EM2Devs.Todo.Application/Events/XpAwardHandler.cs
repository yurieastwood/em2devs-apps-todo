using System.Globalization;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Awards XP when a task is completed, with deadline and streak modifiers.
/// Records the streak completion *before* reading the profile so the multiplier
/// sees the up-to-date streak (Plan 1: this fixes the silent no-op bug where
/// the handler was always passing CurrentStreak=0 to XpCalculator).
/// Publishes LevelUpEvent if the XP gain causes a level up.
/// Attributes XP to parent quest(s) when the task belongs to a quest.
/// Part of the gamification chain: TaskCompleted → XpAwarded → LevelUp (ADR-010, ADR-018).
/// </summary>
public sealed class XpAwardHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly IMediator _mediator;
    private readonly IQuestRepository _questRepository;

    public XpAwardHandler(IPlayerProfileRepository profileRepository, IMediator mediator, IQuestRepository questRepository)
    {
        _profileRepository = profileRepository;
        _mediator = mediator;
        _questRepository = questRepository;
    }

    public async Task Handle(TaskCompletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        DateTimeOffset completedAt = notification.CompletedAt ?? DateTimeOffset.UtcNow;

        PlayerProfileReadModel profileForTz = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);
        TimeZoneInfo userTz = TimeZoneInfo.FindSystemTimeZoneById(profileForTz.TimeZoneId);
        DateOnly completionDate = Streak.ToUserLocalDate(completedAt, userTz);

        await _profileRepository.RecordCompletionAsync(completionDate, ct).ConfigureAwait(false);

        PlayerProfileReadModel profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        XpBreakdown breakdown = XpCalculator.Calculate(
            notification.Difficulty,
            notification.Deadline,
            completedAt,
            profile.CurrentStreak);

        int previousLevel = profile.Level;

        XpBreakdownReadModel breakdownModel = new(
            breakdown.BaseXp, breakdown.DeadlineModifier, breakdown.StreakMultiplier, breakdown.FinalXp);

        await _profileRepository.AwardXpAsync(
            breakdown.ToExperiencePoints(),
            breakdownModel,
            historyDate: completionDate,
            historySource: $"Task completed ({notification.Difficulty})",
            ct: ct).ConfigureAwait(false);

        // Attribute XP to parent quest(s)
        ExperiencePoints earnedXp = breakdown.ToExperiencePoints();
        IReadOnlyList<Quest> quests = await _questRepository
            .GetByTaskIdAsync(notification.TaskId, ct).ConfigureAwait(false);

        foreach (Quest quest in quests)
        {
            quest.AddXpEarned(earnedXp);
            await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
        }

        PlayerProfileReadModel updatedProfile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        if (updatedProfile.Level > previousLevel)
        {
            await _mediator.Publish(new LevelUpEvent(previousLevel, updatedProfile.Level), ct).ConfigureAwait(false);
        }

        StreakMilestone? milestone = StreakMilestone.ForDays(updatedProfile.CurrentStreak);
        if (milestone is not null)
        {
            await _mediator.Publish(
                new StreakMilestoneReachedEvent(milestone.Days, milestone.Label), ct).ConfigureAwait(false);
        }
    }
}
