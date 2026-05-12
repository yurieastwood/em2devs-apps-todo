using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Singleton-keyed store backing <see cref="InMemoryQuestRepository"/>. Keyed by
/// <c>(UserId, QuestId)</c> so per-user reads are O(1) and isolation is enforced
/// without scanning the whole dictionary.
/// </summary>
public sealed class InMemoryQuestStore
{
    public ConcurrentDictionary<(Guid UserId, Guid QuestId), Quest> Quests { get; } = new();
}

public sealed class InMemoryQuestRepository : IQuestRepository
{
    private readonly InMemoryQuestStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryQuestRepository(InMemoryQuestStore store, ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(currentUser);
        _store = store;
        _currentUser = currentUser;
    }

    public Task<Quest?> GetByIdAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        _store.Quests.TryGetValue((_currentUser.UserId, id.Value), out Quest? quest);
        return Task.FromResult(quest);
    }

    public Task<IReadOnlyList<Quest>> GetAllAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        IReadOnlyList<Quest> quests = _store.Quests
            .Where(kvp => kvp.Key.UserId == userId)
            .Select(kvp => kvp.Value)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(quests);
    }

    public Task<IReadOnlyList<Quest>> GetByTaskIdAsync(TaskId taskId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        Guid userId = _currentUser.UserId;
        IReadOnlyList<Quest> quests = _store.Quests
            .Where(kvp => kvp.Key.UserId == userId && kvp.Value.Tasks.Any(t => t.Id == taskId))
            .Select(kvp => kvp.Value)
            .ToList()
            .AsReadOnly();
        return Task.FromResult(quests);
    }

    public Task SaveAsync(Quest quest, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(quest);
        _store.Quests[(_currentUser.UserId, quest.Id.Value)] = quest;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Task.FromResult(_store.Quests.TryRemove((_currentUser.UserId, id.Value), out _));
    }
}
