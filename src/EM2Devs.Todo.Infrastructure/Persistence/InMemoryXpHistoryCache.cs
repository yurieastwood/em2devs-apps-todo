using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Singleton in-memory implementation of <see cref="IXpHistoryCache"/>.
/// Thread-safe via a plain lock (append volume is low).
/// </summary>
public sealed class InMemoryXpHistoryCache : IXpHistoryCache
{
    private readonly object _lock = new();
    private readonly List<XpHistoryEntryReadModel> _entries = [];

    public void Append(DateOnly earnedOn, int xpEarned, string source)
    {
        lock (_lock)
        {
            int cumulative = (_entries.Count == 0 ? 0 : _entries[^1].CumulativeTotal) + xpEarned;
            _entries.Add(new XpHistoryEntryReadModel(earnedOn, xpEarned, source, cumulative));
        }
    }

    public IReadOnlyList<XpHistoryEntryReadModel> GetAll()
    {
        lock (_lock)
        {
            return _entries.ToList().AsReadOnly();
        }
    }
}
