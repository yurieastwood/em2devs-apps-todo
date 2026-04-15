using EM2Devs.Todo.Domain.Entities;

namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Read model for player progression profile, returned by the API.
/// Combines XP, level, streak, XP history, titles, and skill trees into a single view.
/// Backed by the persistent <see cref="PlayerProfile"/> aggregate.
/// </summary>
public sealed record PlayerProfileReadModel(
    int TotalXp,
    int Level,
    int XpToNextLevel,
    int CurrentStreak,
    int LongestStreak,
    XpBreakdownReadModel? LastXpBreakdown = null,
    IReadOnlyList<XpHistoryEntryReadModel>? XpHistory = null,
    TitlesReadModel? Titles = null,
    IReadOnlyList<SkillTreeReadModel>? SkillTrees = null,
    StreakFreezeReadModel? StreakFreeze = null);

/// <summary>
/// Snapshot of the currently active streak freeze, if any. ExpiresAt is the exclusive
/// upper bound: the freeze covers <c>[FrozenAt, ExpiresAt)</c>.
/// </summary>
public sealed record StreakFreezeReadModel(
    DateOnly FrozenAt,
    int Days,
    DateOnly ExpiresAt);

/// <summary>
/// Read model for the last XP breakdown awarded.
/// </summary>
public sealed record XpBreakdownReadModel(
    int BaseXp,
    double DeadlineModifier,
    double StreakMultiplier,
    int FinalXp);

/// <summary>
/// A single entry in the player's XP history log.
/// </summary>
public sealed record XpHistoryEntryReadModel(
    DateOnly Date,
    int XpEarned,
    string Source,
    int CumulativeTotal);

/// <summary>
/// The titles section of the player profile: earned titles, the active title (if any),
/// and progress toward unearned titles.
/// </summary>
public sealed record TitlesReadModel(
    IReadOnlyList<TitleReadModel> Earned,
    string? Active,
    IReadOnlyList<TitleProgressReadModel> Progress);

/// <summary>
/// An earned title with display name and earn date.
/// </summary>
public sealed record TitleReadModel(
    string Type,
    string DisplayName,
    DateOnly EarnedOn);

/// <summary>
/// Progress toward a not-yet-earned title.
/// </summary>
public sealed record TitleProgressReadModel(
    string Type,
    int ProgressPercentage,
    string RemainingDescription);

/// <summary>
/// A single skill tree in the catalog: unlocked entries carry tier progress and perks;
/// locked entries carry an unlock hint.
/// </summary>
public sealed record SkillTreeReadModel(
    string Type,
    int? Tier,
    int? TasksCompletedInTier,
    int? TasksToNextTier,
    string? UnlockHint,
    IReadOnlyList<SkillTreePerkReadModel> Perks);

/// <summary>
/// A perk unlocked at a specific tier of a skill tree.
/// </summary>
public sealed record SkillTreePerkReadModel(
    int Tier,
    string PerkType,
    string Description);
