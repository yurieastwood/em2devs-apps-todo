namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A cohort of users grouped by level range for leaderboard comparison.
/// Users are grouped within 10 levels of each other.
/// Maps to: docs/features/social/leaderboards.feature — "View my leaderboard cohort"
/// </summary>
public sealed record LeaderboardCohort
{
    /// <summary>The level range for grouping users into cohorts (within 10 levels).</summary>
    public const int CohortLevelRange = 10;

    /// <summary>The lowest level in this cohort range.</summary>
    public int MinLevel { get; }

    /// <summary>The highest level in this cohort range.</summary>
    public int MaxLevel { get; }

    private readonly List<LeaderboardEntry> _entries;

    /// <summary>Ranked entries in this cohort (visible entries only, opted-out excluded).</summary>
    public IReadOnlyList<LeaderboardEntry> Entries => _entries.AsReadOnly();

    public LeaderboardCohort(int minLevel, int maxLevel, IEnumerable<LeaderboardEntry> entries)
    {
        if (minLevel < 1)
        {
            throw new Exceptions.DomainException("Minimum level must be at least 1.");
        }

        if (maxLevel < minLevel)
        {
            throw new Exceptions.DomainException("Maximum level cannot be less than minimum level.");
        }

        if (maxLevel - minLevel > CohortLevelRange)
        {
            throw new Exceptions.DomainException(
                $"Cohort level range cannot exceed {CohortLevelRange}.");
        }

        MinLevel = minLevel;
        MaxLevel = maxLevel;
        _entries = entries?.ToList()
            ?? throw new ArgumentNullException(nameof(entries));
    }

    /// <summary>
    /// Determines the cohort range for a given user level.
    /// Cohorts are aligned to ranges: 1-10, 11-20, 21-30, etc.
    /// </summary>
    public static (int minLevel, int maxLevel) CohortRangeForLevel(int level)
    {
        if (level < 1)
        {
            throw new Exceptions.DomainException("Level must be at least 1.");
        }

        int bucket = (level - 1) / CohortLevelRange;
        int min = bucket * CohortLevelRange + 1;
        int max = Math.Min(min + CohortLevelRange - 1, Level.MaxLevel);

        return (min, max);
    }

    /// <summary>
    /// Checks whether a user at the given level belongs in this cohort.
    /// </summary>
    public bool ContainsLevel(int level) =>
        level >= MinLevel && level <= MaxLevel;

    /// <summary>
    /// Returns the top N entries (excluding opted-out users).
    /// </summary>
    public IReadOnlyList<LeaderboardEntry> TopEntries(int count) =>
        _entries.Where(e => !e.IsOptedOut).Take(count).ToList().AsReadOnly();

    /// <summary>
    /// Returns the entry for a specific user, including opted-out users who see a placeholder rank.
    /// </summary>
    public LeaderboardEntry? EntryForUser(string userId) =>
        _entries.Find(e => e.UserId == userId);
}
