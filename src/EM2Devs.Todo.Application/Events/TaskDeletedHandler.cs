using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Removes a deleted task from any quests it belongs to, recalculating quest progress.
/// Maps to: task-management.feature - "Delete a task that belongs to a quest"
/// </summary>
public sealed class TaskDeletedHandler : INotificationHandler<TaskDeletedEvent>
{
    private readonly IQuestRepository _questRepository;

    public TaskDeletedHandler(IQuestRepository questRepository)
    {
        _questRepository = questRepository;
    }

    public async Task Handle(TaskDeletedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        IReadOnlyList<Quest> affectedQuests = await _questRepository
            .GetByTaskIdAsync(notification.TaskId, ct).ConfigureAwait(false);

        foreach (Quest quest in affectedQuests)
        {
            quest.RemoveTask(notification.TaskId);
            await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
        }
    }
}
