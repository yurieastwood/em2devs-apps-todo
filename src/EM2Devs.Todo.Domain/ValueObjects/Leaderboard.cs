namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Manages leaderboard entries, cohort logic, weekly resets, and tie-breaking.
/// Maps to: docs/features/social/leaderboards.feature
/// </summary>
public sealed record Leaderboard
{
    /// <summary>The number of top entries shown by default.</summary>
    public const int DefaultTopCount = 10;

    /// <summary>The type of this leaderboard.</summary>
    public LeaderboardType Type { get; }

    /// <summary>The start of the current week (Monday 00:00 UTC).</summary>
    public DateTimeOffset WeekStart { get; }

    private readonly List<LeaderboardEntry> _entries;

    /// <summary>All entries (before cohort filtering).</summary>
    public IReadOnlyList<LeaderboardEntry> Entries => _entries.AsReadOnly();

    public Leaderboard(LeaderboardType type, DateTimeOffset weekStart, IEnumerable<LeaderboardEntry> entries)
    {
        Type = type;
        WeekStart = weekStart;
        _entries = entries?.ToList()
            ?? throw new ArgumentNullException(nameof(entries));
    }

    /// <summary>
    /// Creates an empty leaderboard for the current week.
    /// </summary>
    public static Leaderboard Create(LeaderboardType type, DateTimeOffset weekStart) =>
        new(type, weekStart, []);

    /// <summary>
    /// Adds an entry to the leaderboard and re-ranks all entries.
    /// Tie-breaking: higher metric first, then earlier achievedAt wins.
    /// </summary>
    public Leaderboard AddEntry(LeaderboardEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        List<LeaderboardEntry> updated = [.. _entries, entry];
        return new Leaderboard(Type, WeekStart, RankEntries(updated));
    }

    /// <summary>
    /// Returns a cohort view filtered by level range and ranked.
    /// </summary>
    public LeaderboardCohort GetCohort(int userLevel)
    {
        (int min, int max) = LeaderboardCohort.CohortRangeForLevel(userLevel);

        List<LeaderboardEntry> cohortEntries = _entries
            .Where(e => e.UserLevel >= min && e.UserLevel <= max)
            .ToList();

        List<LeaderboardEntry> ranked = RankEntries(cohortEntries);

        return new LeaderboardCohort(min, max, ranked);
    }

    /// <summary>
    /// Determines whether the leaderboard should reset given the current time.
    /// Resets occur every Monday at 00:00 UTC.
    /// </summary>
    public static bool ShouldReset(DateTimeOffset now, DateTimeOffset weekStart)
    {
        DateTimeOffset nextMonday = GetNextMondayUtc(weekStart);
        return now >= nextMonday;
    }

    /// <summary>
    /// Resets the leaderboard for a new week. Returns the history of the previous week
    /// and a fresh leaderboard.
    /// </summary>
    public (Leaderboard newLeaderboard, LeaderboardHistory history) Reset(DateTimeOffset newWeekStart)
    {
        var history = new LeaderboardHistory(WeekStart, newWeekStart, Type, _entries);
        var fresh = new Leaderboard(Type, newWeekStart, []);
        return (fresh, history);
    }

    /// <summary>
    /// Gets the Monday 00:00 UTC for the week containing the given timestamp.
    /// </summary>
    public static DateTimeOffset GetCurrentWeekStart(DateTimeOffset timestamp)
    {
        DateTimeOffset utc = timestamp.ToUniversalTime();
        int daysSinceMonday = ((int)utc.DayOfWeek + 6) % 7;
        DateTime monday = utc.Date.AddDays(-daysSinceMonday);
        return new DateTimeOffset(monday, TimeSpan.Zero);
    }

    /// <summary>
    /// Gets the next Monday 00:00 UTC after the given week start.
    /// </summary>
    public static DateTimeOffset GetNextMondayUtc(DateTimeOffset weekStart) =>
        weekStart.AddDays(7);

    /// <summary>
    /// Ranks entries by metric value (descending), breaking ties by earlier achievedAt.
    /// Opted-out users are included but won't appear in visible rankings.
    /// </summary>
    private static List<LeaderboardEntry> RankEntries(List<LeaderboardEntry> entries)
    {
        List<LeaderboardEntry> sorted = entries
            .OrderByDescending(e => e.MetricValue)
            .ThenBy(e => e.AchievedAt)
            .ToList();

        List<LeaderboardEntry> ranked = [];
        for (int i = 0; i < sorted.Count; i++)
        {
            ranked.Add(sorted[i].WithRank(i + 1));
        }

        return ranked;
    }
}
