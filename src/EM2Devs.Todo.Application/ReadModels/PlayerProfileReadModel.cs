using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Read model for player progression profile, returned by the API.
/// Combines XP, level, and streak into a single view.
/// Backed by the persistent <see cref="PlayerProfile"/> aggregate.
/// </summary>
public sealed record PlayerProfileReadModel(
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
