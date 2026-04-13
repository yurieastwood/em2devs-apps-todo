namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Privacy settings controlling leaderboard visibility.
/// Maps to: docs/features/social/leaderboards.feature — "Opt out of leaderboards" and "Anonymous leaderboard participation"
/// </summary>
public sealed record LeaderboardSettings
{
    /// <summary>When true, the user is hidden from all leaderboards but can still spectate.</summary>
    public bool OptedOut { get; }

    /// <summary>When true, the user's profile appears as "Anonymous Questor" hiding username and title.</summary>
    public bool Anonymous { get; }

    public LeaderboardSettings(bool optedOut, bool anonymous)
    {
        OptedOut = optedOut;
        Anonymous = anonymous;
    }

    /// <summary>Default settings: visible and non-anonymous.</summary>
    public static LeaderboardSettings Default() => new(false, false);

    /// <summary>Opt out of all leaderboards.</summary>
    public LeaderboardSettings WithOptOut() => new(true, Anonymous);

    /// <summary>Opt back in to leaderboards.</summary>
    public LeaderboardSettings WithOptIn() => new(false, Anonymous);

    /// <summary>Enable anonymous mode.</summary>
    public LeaderboardSettings WithAnonymous() => new(OptedOut, true);

    /// <summary>Disable anonymous mode.</summary>
    public LeaderboardSettings WithIdentified() => new(OptedOut, false);
}
