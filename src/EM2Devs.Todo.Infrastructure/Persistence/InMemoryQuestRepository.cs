using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryQuestRepository : IQuestRepository
{
    private readonly ConcurrentDictionary<Guid, Quest> _store = new();

    public Task<Quest?> GetByIdAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.TryGetValue(id.Value, out Quest? quest);
        return Task.FromResult(quest);
    }

    public Task<IReadOnlyList<Quest>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Quest> quests = _store.Values.ToList().AsReadOnly();
        return Task.FromResult(quests);
    }

    public Task SaveAsync(Quest quest, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(quest);
        _store[quest.Id.Value] = quest;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.TryRemove(id.Value, out _));
    }
}
