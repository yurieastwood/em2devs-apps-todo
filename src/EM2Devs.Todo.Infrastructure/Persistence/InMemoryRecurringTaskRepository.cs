using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Shared-state backing store for <see cref="InMemoryRecurringTaskRepository"/>.
/// Registered as a singleton so multiple scoped repository instances see the same data,
/// while the repository itself is scoped to pick up the scoped <see cref="ICurrentUser"/>.
/// </summary>
public sealed class InMemoryRecurringTaskStore
{
    public ConcurrentDictionary<Guid, RecurringTask> RecurringTasks { get; } = new();
}

public sealed class InMemoryRecurringTaskRepository : IRecurringTaskRepository
{
    private readonly InMemoryRecurringTaskStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryRecurringTaskRepository(InMemoryRecurringTaskStore store, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(currentUser);
        _store = store;
        _currentUser = currentUser;
    }

    public Task<RecurringTask?> GetByIdAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.RecurringTasks.TryGetValue(id.Value, out RecurringTask? task);
        if (task is null || task.UserId != _currentUser.UserId)
        {
            return Task.FromResult<RecurringTask?>(null);
        }
        return Task.FromResult<RecurringTask?>(task);
    }

    public Task<IReadOnlyList<RecurringTask>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<RecurringTask> tasks = _store.RecurringTasks.Values
            .Where(r => r.UserId == _currentUser.UserId)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(tasks);
    }

    public Task SaveAsync(RecurringTask recurringTask, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(recurringTask);
        if (recurringTask.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "RecurringTask UserId does not match the current user. Cross-user writes are forbidden.");
        }

        _store.RecurringTasks[recurringTask.Id.Value] = recurringTask;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (!_store.RecurringTasks.TryGetValue(id.Value, out RecurringTask? task)
            || task.UserId != _currentUser.UserId)
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(_store.RecurringTasks.TryRemove(id.Value, out _));
    }

    public Task<IReadOnlyList<RecurringTask>> GetAllForGenerationAsync(CancellationToken ct = default)
    {
        IReadOnlyList<RecurringTask> tasks = _store.RecurringTasks.Values.ToList().AsReadOnly();
        return Task.FromResult(tasks);
    }
}
