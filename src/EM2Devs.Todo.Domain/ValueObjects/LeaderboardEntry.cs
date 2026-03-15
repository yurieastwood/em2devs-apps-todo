namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A single entry on a leaderboard with rank and score.
/// </summary>
public sealed record LeaderboardEntry
{
    public Guid UserId { get; }
    public int Rank { get; }
    public int Score { get; }
    public int UserLevel { get; }
    public LeaderboardVisibility Visibility { get; }

    public LeaderboardEntry(
        Guid userId, int rank, int score, int userLevel, LeaderboardVisibility visibility)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("User ID cannot be empty.");
        }

        if (rank < 1)
        {
            throw new Exceptions.DomainException("Rank must be at least 1.");
        }

        if (score < 0)
        {
            throw new Exceptions.DomainException("Score cannot be negative.");
        }

        if (userLevel < 1)
        {
            throw new Exceptions.DomainException("User level must be at least 1.");
        }

        UserId = userId;
        Rank = rank;
        Score = score;
        UserLevel = userLevel;
        Visibility = visibility;
    }

    public string DisplayName(string username) =>
        Visibility switch
        {
            LeaderboardVisibility.Public => username,
            LeaderboardVisibility.Anonymous => "Anonymous Questor",
            _ => "---"
        };

    public bool IsVisible => Visibility != LeaderboardVisibility.OptedOut;
}
