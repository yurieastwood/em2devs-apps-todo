using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Singleton-keyed store backing <see cref="InMemoryEpicRepository"/>. Keyed by
/// <c>(UserId, EpicId)</c> so per-user reads are O(1) and isolation is enforced
/// without scanning the whole dictionary.
/// </summary>
public sealed class InMemoryEpicStore
{
    public ConcurrentDictionary<(Guid UserId, Guid EpicId), Epic> Epics { get; } = new();
}

public sealed class InMemoryEpicRepository : IEpicRepository
{
    private readonly InMemoryEpicStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryEpicRepository(InMemoryEpicStore store, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(currentUser);
        _store = store;
        _currentUser = currentUser;
    }

    public Task<Epic?> GetByIdAsync(EpicId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.Epics.TryGetValue((_currentUser.UserId, id.Value), out Epic? epic);
        return Task.FromResult(epic);
    }

    public Task<IReadOnlyList<Epic>> GetAllAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        IReadOnlyList<Epic> epics = _store.Epics
            .Where(kvp => kvp.Key.UserId == userId)
            .Select(kvp => kvp.Value)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(epics);
    }

    public Task SaveAsync(Epic epic, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(epic);
        _store.Epics[(_currentUser.UserId, epic.Id.Value)] = epic;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(EpicId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.Epics.TryRemove((_currentUser.UserId, id.Value), out _));
    }
}
