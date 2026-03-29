using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IRecurringTaskRepository
{
    Task<RecurringTask?> GetByIdAsync(RecurringTaskId id, CancellationToken ct = default);
    Task<IReadOnlyList<RecurringTask>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(RecurringTask recurringTask, CancellationToken ct = default);
    Task<bool> DeleteAsync(RecurringTaskId id, CancellationToken ct = default);
}
