using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.Events;

/// <summary>
/// Syncs quest task snapshots and progress when a task's status changes.
/// Loads the fresh task from the repository and replaces the stale snapshot in each quest.
/// </summary>
public sealed class QuestProgressHandler : INotificationHandler<TaskStatusChangedEvent>
{
    private readonly IQuestRepository _questRepository;
    private readonly ITaskRepository _taskRepository;

    public QuestProgressHandler(IQuestRepository questRepository, ITaskRepository taskRepository)
    {
        _questRepository = questRepository;
        _taskRepository = taskRepository;
    }

    public async Task Handle(TaskStatusChangedEvent notification, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(notification);

        TodoTask? freshTask = await _taskRepository.GetByIdAsync(notification.TaskId, ct).ConfigureAwait(false);
        if (freshTask is null)
        {
            return;
        }

        IReadOnlyList<Quest> affectedQuests = await _questRepository
            .GetByTaskIdAsync(notification.TaskId, ct).ConfigureAwait(false);

        foreach (Quest quest in affectedQuests)
        {
            quest.ReplaceTask(freshTask);
            await _questRepository.SaveAsync(quest, ct).ConfigureAwait(false);
        }
    }
}
