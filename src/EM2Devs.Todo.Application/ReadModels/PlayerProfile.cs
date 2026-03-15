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
    int LongestStreak);
