namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Historical record of a past week's leaderboard results.
/// Maps to: docs/features/social/leaderboards.feature — "View past leaderboard results" (Leaderboard resets weekly)
/// </summary>
public sealed record LeaderboardHistory
{
    /// <summary>The start of the week (Monday 00:00 UTC) this result covers.</summary>
    public DateTimeOffset WeekStart { get; }

    /// <summary>The end of the week (following Monday 00:00 UTC).</summary>
    public DateTimeOffset WeekEnd { get; }

    /// <summary>The type of leaderboard.</summary>
    public LeaderboardType Type { get; }

    private readonly List<LeaderboardEntry> _finalStandings;

    /// <summary>The final standings at the end of the week.</summary>
    public IReadOnlyList<LeaderboardEntry> FinalStandings => _finalStandings.AsReadOnly();

    public LeaderboardHistory(
        DateTimeOffset weekStart,
        DateTimeOffset weekEnd,
        LeaderboardType type,
        IEnumerable<LeaderboardEntry> finalStandings)
    {
        if (weekEnd <= weekStart)
        {
            throw new Exceptions.DomainException("Week end must be after week start.");
        }

        WeekStart = weekStart;
        WeekEnd = weekEnd;
        Type = type;
        _finalStandings = finalStandings?.ToList()
            ?? throw new ArgumentNullException(nameof(finalStandings));
    }
}
