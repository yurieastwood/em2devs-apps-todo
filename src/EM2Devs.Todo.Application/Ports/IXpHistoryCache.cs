using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Singleton in-memory cache for XP history entries, mirroring the
/// <see cref="ILastXpBreakdownCache"/> pattern. The Postgres PlayerProfile
/// repository reloads the aggregate per request and EF ignores the
/// XpHistory owned collection, so we accumulate entries here to survive
/// across requests in the demo. UI-only state; not durable.
///
/// Slice 3 multi-user isolation: entries are partitioned per user so each
/// authenticated user sees only their own XP history.
/// </summary>
public interface IXpHistoryCache
{
    /// <summary>Append an entry for the given user. Cumulative total is computed from prior entries of that user.</summary>
    void Append(Guid userId, DateOnly earnedOn, int xpEarned, string source);

    /// <summary>All entries for the given user in the order they were appended.</summary>
    IReadOnlyList<XpHistoryEntryReadModel> GetForUser(Guid userId);
}
