using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryEpicStore
{
    public ConcurrentDictionary<Guid, Epic> Epics { get; } = new();
}

public sealed class InMemoryEpicRepository : IEpicRepository
{
    private readonly InMemoryEpicStore _store;

    public InMemoryEpicRepository(InMemoryEpicStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    public Task<Epic?> GetByIdAsync(EpicId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.Epics.TryGetValue(id.Value, out Epic? epic);
        return Task.FromResult(epic);
    }

    public Task<IReadOnlyList<Epic>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Epic> epics = _store.Epics.Values.ToList().AsReadOnly();
        return Task.FromResult(epics);
    }

    public Task SaveAsync(Epic epic, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(epic);
        _store.Epics[epic.Id.Value] = epic;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(EpicId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.Epics.TryRemove(id.Value, out _));
    }
}
