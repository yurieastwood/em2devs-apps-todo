namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A single entry in the seasonal leaderboard.
/// Users are ranked by seasonal XP within a level cohort (users within 5 levels).
/// </summary>
public sealed record SeasonalLeaderboardEntry
{
    public const int CohortLevelRange = 5;

    public string UserId { get; }
    public ExperiencePoints SeasonalXp { get; }
    public int Rank { get; }
    public int UserLevel { get; }

    public SeasonalLeaderboardEntry(string userId, ExperiencePoints seasonalXp, int rank, int userLevel)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new Exceptions.DomainException("User ID cannot be empty.");
        }

        ArgumentNullException.ThrowIfNull(seasonalXp);

        if (rank < 1)
        {
            throw new Exceptions.DomainException("Rank must be at least 1.");
        }

        if (userLevel < 1)
        {
            throw new Exceptions.DomainException("User level must be at least 1.");
        }

        UserId = userId;
        SeasonalXp = seasonalXp;
        Rank = rank;
        UserLevel = userLevel;
    }

    /// <summary>
    /// Determines whether two users are in the same cohort (within 5 levels).
    /// </summary>
    public bool IsInCohort(int otherUserLevel) =>
        Math.Abs(UserLevel - otherUserLevel) <= CohortLevelRange;
}
