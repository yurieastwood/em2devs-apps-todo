using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryInsightCardStore
{
    private readonly Dictionary<Guid, List<InsightCard>> _cards = new();
    private readonly object _lock = new();

    public IReadOnlyList<InsightCard> GetForUser(Guid userId, bool includeRead)
    {
        lock (_lock)
        {
            if (!_cards.TryGetValue(userId, out List<InsightCard>? list))
            {
                return [];
            }

            return list
                .Where(c => c.Status != InsightCardStatus.Dismissed
                    && (includeRead || c.Status == InsightCardStatus.Unread || c.Status == InsightCardStatus.Saved))
                .ToList()
                .AsReadOnly();
        }
    }

    public InsightCard? GetById(Guid userId, InsightCardId id)
    {
        lock (_lock)
        {
            return _cards.TryGetValue(userId, out List<InsightCard>? list)
                ? list.FirstOrDefault(c => c.Id == id)
                : null;
        }
    }

    public void Add(Guid userId, InsightCard card)
    {
        lock (_lock)
        {
            if (!_cards.TryGetValue(userId, out List<InsightCard>? list))
            {
                list = [];
                _cards[userId] = list;
            }

            list.Add(card);
        }
    }

    public void RemoveAllForUser(Guid userId)
    {
        lock (_lock)
        {
            _cards.Remove(userId);
        }
    }
}

public sealed class InMemoryInsightCardRepository : IInsightCardRepository
{
    private readonly InMemoryInsightCardStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryInsightCardRepository(InMemoryInsightCardStore store, ICurrentUser currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<InsightCard>> GetForCurrentUserAsync(bool includeRead, CancellationToken ct = default)
        => Task.FromResult(_store.GetForUser(_currentUser.UserId, includeRead));

    public Task<InsightCard?> GetByIdAsync(InsightCardId id, CancellationToken ct = default)
        => Task.FromResult(_store.GetById(_currentUser.UserId, id));

    public Task AddAsync(InsightCard card, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(card);
        _store.Add(_currentUser.UserId, card);
        return Task.CompletedTask;
    }

    public Task SaveAsync(InsightCard card, CancellationToken ct = default)
        => Task.CompletedTask;
}
