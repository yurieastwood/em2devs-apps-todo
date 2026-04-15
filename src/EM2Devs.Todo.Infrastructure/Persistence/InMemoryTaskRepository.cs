using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Shared-state backing store for <see cref="InMemoryTaskRepository"/>.
/// Registered as a singleton so multiple scoped repository instances see the same data,
/// while the repository itself is scoped to pick up the scoped <see cref="ICurrentUser"/>.
/// </summary>
public sealed class InMemoryTaskStore
{
    public ConcurrentDictionary<Guid, TodoTask> Tasks { get; } = new();
}

public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly InMemoryTaskStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryTaskRepository(InMemoryTaskStore store, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(currentUser);
        _store = store;
        _currentUser = currentUser;
    }

    public Task<TodoTask?> GetByIdAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.Tasks.TryGetValue(id.Value, out TodoTask? task);
        if (task is null || task.UserId != _currentUser.UserId)
        {
            return Task.FromResult<TodoTask?>(null);
        }
        return Task.FromResult<TodoTask?>(task);
    }

    public Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<TodoTask> tasks = _store.Tasks.Values
            .Where(t => t.UserId == _currentUser.UserId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(tasks);
    }

    public Task SaveAsync(TodoTask task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (task.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "Task UserId does not match the current user. Cross-user writes are forbidden.");
        }

        _store.Tasks[task.Id.Value] = task;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (!_store.Tasks.TryGetValue(id.Value, out TodoTask? task) || task.UserId != _currentUser.UserId)
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(_store.Tasks.TryRemove(id.Value, out _));
    }

    public Task<IReadOnlyList<TodoTask>> GetByRecurringTaskIdAsync(RecurringTaskId sourceId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        IReadOnlyList<TodoTask> tasks = _store.Tasks.Values
            .Where(t => t.UserId == _currentUser.UserId && t.SourceRecurringTaskId == sourceId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(tasks);
    }

    public Task<DateOnly?> GetMaxScheduledDateAsync(RecurringTaskId sourceId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        DateOnly? max = _store.Tasks.Values
            .Where(t => t.UserId == _currentUser.UserId && t.SourceRecurringTaskId == sourceId)
            .Select(t => t.ScheduledDate)
            .Max();
        return Task.FromResult(max);
    }

    public Task SaveForGenerationAsync(TodoTask task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        _store.Tasks[task.Id.Value] = task;
        return Task.CompletedTask;
    }

    public Task<DateOnly?> GetMaxScheduledDateForGenerationAsync(RecurringTaskId sourceId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        DateOnly? max = _store.Tasks.Values
            .Where(t => t.SourceRecurringTaskId == sourceId)
            .Select(t => t.ScheduledDate)
            .Max();
        return Task.FromResult(max);
    }
}
