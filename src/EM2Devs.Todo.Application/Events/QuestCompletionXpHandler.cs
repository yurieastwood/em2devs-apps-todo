using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Awards quest completion bonus XP when a quest is completed.
/// Maps to: task-management.feature - "Complete the final task in a quest"
/// </summary>
public sealed class QuestCompletionXpHandler : INotificationHandler<QuestCompletedEvent>
{
    /// <summary>
    /// Flat bonus XP awarded for completing a quest.
    /// </summary>
    public static readonly ExperiencePoints QuestCompletionBonusXp = new(50);

    private readonly IPlayerProfileRepository _profileRepository;

    public QuestCompletionXpHandler(IPlayerProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task Handle(QuestCompletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        await _profileRepository.AwardXpAsync(QuestCompletionBonusXp, breakdown: null, ct).ConfigureAwait(false);
    }
}
