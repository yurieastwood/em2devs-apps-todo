namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Read model for player progression profile.
/// Combines XP, level, and streak into a single view.
/// </summary>
public sealed record PlayerProfile(
    int TotalXp,
    int Level,
    int XpToNextLevel,
    int CurrentStreak,
    int LongestStreak,
    XpBreakdownReadModel? LastXpBreakdown = null);

/// <summary>
/// Read model for the last XP breakdown awarded.
/// </summary>
public sealed record XpBreakdownReadModel(
    int BaseXp,
    double DeadlineModifier,
    double StreakMultiplier,
    int FinalXp);
