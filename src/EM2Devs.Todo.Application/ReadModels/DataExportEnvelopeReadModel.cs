namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Full data export envelope for the authenticated user.
/// Mirrors the OpenAPI <c>DataExportResponse</c> schema in <c>docs/contracts/openapi.yaml</c>.
/// Each inner array contains flat snapshot records — domain entities are mapped to
/// these DTOs in <c>ExportDataQueryHandler</c> so the wire format never leaks
/// value-object ID wrappers or change-tracking metadata.
/// </summary>
public sealed record DataExportEnvelopeReadModel(
    DataExportMetaSection Meta,
    IReadOnlyList<TaskExportSnapshot> Tasks,
    IReadOnlyList<QuestExportSnapshot> Quests,
    IReadOnlyList<EpicExportSnapshot> Epics,
    IReadOnlyList<System.Text.Json.JsonElement> Sagas,
    IReadOnlyList<XpHistoryEntryReadModel> XpHistory,
    DataExportLevelSection Level,
    IReadOnlyList<SkillTreeReadModel> SkillTreeProgress,
    IReadOnlyList<TitleReadModel> TitlesEarned,
    IReadOnlyList<WeeklyReflectionSnapshot> WeeklyReviews,
    IReadOnlyList<TimelineEventReadModel> TimelineEvents,
    IReadOnlyList<InsightCardReadModel> InsightCards,
    DataExportSettingsSection Settings);

public sealed record DataExportMetaSection(
    DateTimeOffset ExportedAt,
    string Format,
    string Scope,
    int RecordCount);

/// <summary>
/// Per the OpenAPI contract (DataExportLevel), each field is optional on the wire —
/// absence is treated as the starting-state defaults (current=1, xp=0, longestStreak=0).
/// Nullable fields preserve that distinction: <c>null</c> = absent (default applied),
/// non-null = explicit value (validated against schema bounds).
/// </summary>
public sealed record DataExportLevelSection(
    int? Current,
    int? Xp,
    int? LongestStreak);

/// <summary>
/// Per-user settings. Each field is null until the corresponding feature is wired
/// to persistence (the four settings value objects exist in Domain but no settings
/// repository has been built yet). Typed as a string-keyed dictionary so JSON
/// deserialization rejects non-object payloads (arrays, scalars) — required by the
/// OpenAPI schema and enforced by Schemathesis property tests.
/// </summary>
public sealed record DataExportSettingsSection(
    Dictionary<string, System.Text.Json.JsonElement>? DataPrivacy,
    Dictionary<string, System.Text.Json.JsonElement>? Notifications,
    Dictionary<string, System.Text.Json.JsonElement>? Sync,
    Dictionary<string, System.Text.Json.JsonElement>? Leaderboard);

public sealed record TaskExportSnapshot(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Difficulty,
    string Priority,
    bool IsBossTask,
    int? EstimatedMinutes,
    int? ActualMinutes,
    DateTimeOffset? DueDate,
    DateOnly? ScheduledDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    Guid? AssignedQuestId,
    string[] Tags);

public sealed record QuestExportSnapshot(
    Guid Id,
    string Title,
    string Description,
    DateOnly? DueDate,
    bool IsCompleted,
    int TotalXpEarned,
    Guid? AssignedEpicId,
    Guid[] TaskIds);

public sealed record EpicExportSnapshot(
    Guid Id,
    string Title,
    string Description,
    DateOnly? TargetDate,
    bool IsCompleted,
    Guid[] QuestIds);
