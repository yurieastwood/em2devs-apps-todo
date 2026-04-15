using System.Collections.Concurrent;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Singleton in-memory implementation of <see cref="IXpHistoryCache"/>.
/// Thread-safe: a <see cref="ConcurrentDictionary{TKey,TValue}"/> keyed by UserId,
/// with a lock around each user's list for append/read consistency.
/// </summary>
public sealed class InMemoryXpHistoryCache : IXpHistoryCache
{
    private readonly ConcurrentDictionary<Guid, List<XpHistoryEntryReadModel>> _entries = new();

    public void Append(Guid userId, DateOnly earnedOn, int xpEarned, string source)
    {
        List<XpHistoryEntryReadModel> list = _entries.GetOrAdd(userId, _ => []);
        lock (list)
        {
            int cumulative = (list.Count == 0 ? 0 : list[^1].CumulativeTotal) + xpEarned;
            list.Add(new XpHistoryEntryReadModel(earnedOn, xpEarned, source, cumulative));
        }
    }

    public IReadOnlyList<XpHistoryEntryReadModel> GetForUser(Guid userId)
    {
        if (!_entries.TryGetValue(userId, out List<XpHistoryEntryReadModel>? list))
        {
            return Array.Empty<XpHistoryEntryReadModel>();
        }

        lock (list)
        {
            return list.ToList().AsReadOnly();
        }
    }
}
