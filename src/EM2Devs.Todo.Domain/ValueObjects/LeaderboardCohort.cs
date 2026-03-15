namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Cohort-based leaderboard grouping users within a level range.
/// Users are compared only against others within 10 levels.
/// </summary>
public sealed record LeaderboardCohort
{
    public const int CohortRange = 10;

    public int MinLevel { get; }
    public int MaxLevel { get; }

    public LeaderboardCohort(int minLevel, int maxLevel)
    {
        if (minLevel < 1)
        {
            throw new Exceptions.DomainException("Minimum level must be at least 1.");
        }

        if (maxLevel < minLevel)
        {
            throw new Exceptions.DomainException(
                "Maximum level must be greater than or equal to minimum level.");
        }

        MinLevel = minLevel;
        MaxLevel = maxLevel;
    }

    public static LeaderboardCohort ForUserLevel(int userLevel)
    {
        if (userLevel < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(userLevel), userLevel, "User level must be at least 1.");
        }

        int min = Math.Max(1, userLevel - CohortRange);
        int max = userLevel + CohortRange;
        return new LeaderboardCohort(min, max);
    }

    public bool IncludesLevel(int level) =>
        level >= MinLevel && level <= MaxLevel;
}
