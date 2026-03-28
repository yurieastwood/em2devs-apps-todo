using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Recalculates quest progress when a task's status changes.
/// Ensures quests stay in sync with their tasks regardless of how the task was updated.
/// </summary>
public sealed class QuestProgressHandler : INotificationHandler<TaskStatusChangedEvent>
{
    private readonly IQuestRepository _questRepository;

    public QuestProgressHandler(IQuestRepository questRepository)
    {
        _questRepository = questRepository;
    }

    public async Task Handle(TaskStatusChangedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        IReadOnlyList<Quest> affectedQuests = await _questRepository
            .GetByTaskIdAsync(notification.TaskId, ct).ConfigureAwait(false);

        foreach (Quest quest in affectedQuests)
        {
            await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
        }
    }
}
