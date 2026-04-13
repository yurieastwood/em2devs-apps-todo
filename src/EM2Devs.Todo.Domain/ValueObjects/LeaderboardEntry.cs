namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A single entry in a weekly leaderboard.
/// Tracks a user's metric value, rank, display name, and the timestamp they achieved their score
/// (used for tie-breaking: earlier achievement wins).
/// Maps to: docs/features/social/leaderboards.feature
/// </summary>
public sealed record LeaderboardEntry
{
    public const string AnonymousDisplayName = "Anonymous Questor";

    public string UserId { get; }
    public string DisplayName { get; }
    public int MetricValue { get; }
    public int Rank { get; }
    public int UserLevel { get; }
    public DateTimeOffset AchievedAt { get; }
    public bool IsOptedOut { get; }
    public bool IsAnonymous { get; }

    public LeaderboardEntry(
        string userId,
        string displayName,
        int metricValue,
        int rank,
        int userLevel,
        DateTimeOffset achievedAt,
        bool isOptedOut = false,
        bool isAnonymous = false)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new Exceptions.DomainException("User ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new Exceptions.DomainException("Display name cannot be empty.");
        }

        if (metricValue < 0)
        {
            throw new Exceptions.DomainException("Metric value cannot be negative.");
        }

        if (rank < 1)
        {
            throw new Exceptions.DomainException("Rank must be at least 1.");
        }

        if (userLevel < 1)
        {
            throw new Exceptions.DomainException("User level must be at least 1.");
        }

        UserId = userId;
        DisplayName = isAnonymous ? AnonymousDisplayName : displayName;
        MetricValue = metricValue;
        Rank = rank;
        UserLevel = userLevel;
        AchievedAt = achievedAt;
        IsOptedOut = isOptedOut;
        IsAnonymous = isAnonymous;
    }

    /// <summary>
    /// Returns a copy with a new rank value.
    /// </summary>
    public LeaderboardEntry WithRank(int newRank)
    {
        return new LeaderboardEntry(UserId, DisplayName, MetricValue, newRank, UserLevel, AchievedAt, IsOptedOut, IsAnonymous);
    }
}
