namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Read model for the weekly review ritual surfaced by <c>GET /api/weekly-review</c>.
/// Combines a week-of summary (tasks completed, XP earned, streak delta) with any
/// previously-saved reflection for that week.
/// </summary>
public sealed record WeeklyReviewReadModel(
    DateOnly WeekOf,
    int TasksCompleted,
    int XpEarned,
    int StreakStart,
    int StreakEnd,
    IReadOnlyList<string> NotableEvents,
    WeeklyReflectionReadModel? Reflection);

/// <summary>
/// The user-authored reflection for a given week. Absent when the user
/// has not yet saved anything for this week.
/// </summary>
public sealed record WeeklyReflectionReadModel(
    string WhatWentWell,
    string WhatDragged,
    string Adjustment,
    DateTimeOffset SavedAt);
