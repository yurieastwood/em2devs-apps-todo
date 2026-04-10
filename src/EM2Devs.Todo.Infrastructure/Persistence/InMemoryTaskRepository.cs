using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly ConcurrentDictionary<Guid, TodoTask> _store = new();

    public Task<TodoTask?> GetByIdAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.TryGetValue(id.Value, out var task);
        return Task.FromResult(task);
    }

    public Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<TodoTask> tasks = _store.Values.ToList().AsReadOnly();
        return Task.FromResult(tasks);
    }

    public Task SaveAsync(TodoTask task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        _store[task.Id.Value] = task;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.TryRemove(id.Value, out _));
    }

    public Task<IReadOnlyList<TodoTask>> GetByRecurringTaskIdAsync(RecurringTaskId sourceId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        IReadOnlyList<TodoTask> tasks = _store.Values
            .Where(t => t.SourceRecurringTaskId == sourceId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(tasks);
    }

    public Task<DateOnly?> GetMaxScheduledDateAsync(RecurringTaskId sourceId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        DateOnly? max = _store.Values
            .Where(t => t.SourceRecurringTaskId == sourceId && t.ScheduledDate is not null)
            .Select(t => t.ScheduledDate!.Value)
            .DefaultIfEmpty()
            .Max();
        return Task.FromResult(max == default ? null : (DateOnly?)max);
    }
}
