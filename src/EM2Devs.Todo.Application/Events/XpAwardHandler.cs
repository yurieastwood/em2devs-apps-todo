using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Awards XP when a task is completed, with deadline and streak modifiers.
/// Publishes LevelUpEvent if the XP gain causes a level up.
/// Part of the gamification chain: TaskCompleted → XpAwarded → LevelUp (ADR-010, ADR-018).
/// </summary>
public sealed class XpAwardHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly IMediator _mediator;

    public XpAwardHandler(IPlayerProfileRepository profileRepository, IMediator mediator)
    {
        _profileRepository = profileRepository;
        _mediator = mediator;
    }

    public async Task Handle(TaskCompletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        PlayerProfile profile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        XpBreakdown breakdown = XpCalculator.Calculate(
            notification.Difficulty,
            notification.Deadline,
            notification.CompletedAt ?? DateTimeOffset.UtcNow,
            profile.CurrentStreak);

        int previousLevel = profile.Level;

        XpBreakdownReadModel breakdownModel = new(
            breakdown.BaseXp, breakdown.DeadlineModifier, breakdown.StreakMultiplier, breakdown.FinalXp);

        await _profileRepository.AwardXpAsync(breakdown.ToExperiencePoints(), breakdownModel, ct).ConfigureAwait(false);

        PlayerProfile updatedProfile = await _profileRepository.GetProfileAsync(ct).ConfigureAwait(false);

        if (updatedProfile.Level > previousLevel)
        {
            await _mediator.Publish(new LevelUpEvent(previousLevel, updatedProfile.Level), ct).ConfigureAwait(false);
        }
    }
}
