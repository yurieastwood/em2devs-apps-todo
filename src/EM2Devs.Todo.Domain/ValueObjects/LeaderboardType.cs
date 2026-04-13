namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// The different types of leaderboards available in Waypoint.
/// Maps to: docs/features/social/leaderboards.feature — "View a leaderboard by type"
/// </summary>
public enum LeaderboardType
{
    /// <summary>Ranks users by XP earned in the current week.</summary>
    WeeklyXP,

    /// <summary>Ranks users by their current active streak length.</summary>
    LongestStreak,

    /// <summary>Ranks users by quests completed this season.</summary>
    QuestCloser,

    /// <summary>Ranks guild members by contribution this week.</summary>
    Guild,
}
