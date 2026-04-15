using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IRecurringTaskRepository
{
    Task<RecurringTask?> GetByIdAsync(RecurringTaskId id, CancellationToken ct = default);
    Task<IReadOnlyList<RecurringTask>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(RecurringTask recurringTask, CancellationToken ct = default);
    Task<bool> DeleteAsync(RecurringTaskId id, CancellationToken ct = default);

    /// <summary>
    /// Returns every recurring task across all users, bypassing the current-user filter.
    /// Intended only for the background generation job, which iterates templates and
    /// generates instances on behalf of each owning user.
    /// </summary>
    Task<IReadOnlyList<RecurringTask>> GetAllForGenerationAsync(CancellationToken ct = default);
}
