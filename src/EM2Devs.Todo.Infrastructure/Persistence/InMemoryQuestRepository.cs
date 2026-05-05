using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryQuestStore
{
    public ConcurrentDictionary<Guid, Quest> Quests { get; } = new();
}

public sealed class InMemoryQuestRepository : IQuestRepository
{
    private readonly InMemoryQuestStore _store;

    public InMemoryQuestRepository(InMemoryQuestStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    public Task<Quest?> GetByIdAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.Quests.TryGetValue(id.Value, out Quest? quest);
        return Task.FromResult(quest);
    }

    public Task<IReadOnlyList<Quest>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Quest> quests = _store.Quests.Values.ToList().AsReadOnly();
        return Task.FromResult(quests);
    }

    public Task<IReadOnlyList<Quest>> GetByTaskIdAsync(TaskId taskId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        IReadOnlyList<Quest> quests = _store.Quests.Values
            .Where(q => q.Tasks.Any(t => t.Id == taskId))
            .ToList()
            .AsReadOnly();
        return Task.FromResult(quests);
    }

    public Task SaveAsync(Quest quest, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(quest);
        _store.Quests[quest.Id.Value] = quest;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.Quests.TryRemove(id.Value, out _));
    }
}
