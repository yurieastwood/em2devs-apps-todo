using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryRecurringTaskRepository : IRecurringTaskRepository
{
    private readonly ConcurrentDictionary<Guid, RecurringTask> _store = new();

    public Task<RecurringTask?> GetByIdAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.TryGetValue(id.Value, out RecurringTask? task);
        return Task.FromResult(task);
    }

    public Task<IReadOnlyList<RecurringTask>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<RecurringTask> tasks = _store.Values.ToList().AsReadOnly();
        return Task.FromResult(tasks);
    }

    public Task SaveAsync(RecurringTask recurringTask, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(recurringTask);
        _store[recurringTask.Id.Value] = recurringTask;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.TryRemove(id.Value, out _));
    }
}
