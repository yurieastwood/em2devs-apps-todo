using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface ITaskRepository
{
    Task<TodoTask?> GetByIdAsync(TaskId id, CancellationToken ct = default);
    Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(TodoTask task, CancellationToken ct = default);
    Task<bool> DeleteAsync(TaskId id, CancellationToken ct = default);
    Task<IReadOnlyList<TodoTask>> GetByRecurringTaskIdAsync(RecurringTaskId sourceId, CancellationToken ct = default);
    Task<DateOnly?> GetMaxScheduledDateAsync(RecurringTaskId sourceId, CancellationToken ct = default);

    /// <summary>
    /// Saves a task spawned by the background generation job on behalf of its owning user,
    /// bypassing the current-user ownership assertion. The task must already carry the correct
    /// <c>UserId</c> — the caller is the recurring-task generator, not an authenticated user.
    /// </summary>
    Task SaveForGenerationAsync(TodoTask task, CancellationToken ct = default);

    /// <summary>
    /// Returns the max scheduled date for instances of the given recurring task across all users.
    /// Intended only for the background generation job, which runs outside a user scope.
    /// </summary>
    Task<DateOnly?> GetMaxScheduledDateForGenerationAsync(RecurringTaskId sourceId, CancellationToken ct = default);
}
