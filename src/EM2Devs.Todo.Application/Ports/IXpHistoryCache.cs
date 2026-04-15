using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Singleton in-memory cache for XP history entries, mirroring the
/// <see cref="ILastXpBreakdownCache"/> pattern. The Postgres PlayerProfile
/// repository reloads the aggregate per request and EF ignores the
/// XpHistory owned collection, so we accumulate entries here to survive
/// across requests in the demo. UI-only state; not durable.
/// </summary>
public interface IXpHistoryCache
{
    /// <summary>Append an entry. Cumulative total is computed from prior entries.</summary>
    void Append(DateOnly earnedOn, int xpEarned, string source);

    /// <summary>All entries in the order they were appended.</summary>
    IReadOnlyList<XpHistoryEntryReadModel> GetAll();
}
