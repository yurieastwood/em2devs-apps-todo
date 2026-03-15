using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Awards XP when a task is completed.
/// Part of the gamification chain: TaskCompleted → XpAwarded (ADR-010).
/// </summary>
public sealed class XpAwardHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly IPlayerProfileRepository _profileRepository;

    public XpAwardHandler(IPlayerProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task Handle(TaskCompletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        ExperiencePoints xp = ExperiencePoints.BaseForDifficulty(TaskDifficulty.Normal);
        await _profileRepository.AwardXpAsync(xp, ct).ConfigureAwait(false);
    }
}
