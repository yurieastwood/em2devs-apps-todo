namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Read model for the stateless daily brief surfaced by <c>GET /api/daily-brief</c>.
/// Every call recomputes "today's brief" from current task data and player profile —
/// no brief is persisted for this slice. <see cref="Status"/> signals whether the brief
/// has enough content to display.
/// </summary>
public sealed record DailyBriefReadModel(
    DateOnly Date,
    string Greeting,
    int CurrentStreakDays,
    int CorePlanCount,
    int IfTimeAllowsCount,
    int OverdueCount,
    IReadOnlyList<DailyBriefTaskReadModel> CorePlan,
    IReadOnlyList<DailyBriefTaskReadModel> IfTimeAllows,
    IReadOnlyList<DailyBriefTaskReadModel> Overdue,
    string Status);

/// <summary>
/// Projection of a single task inside a daily brief — the minimum shape needed
/// to render a brief row on the dashboard.
/// </summary>
public sealed record DailyBriefTaskReadModel(
    Guid Id,
    string Title,
    string Difficulty,
    string Priority,
    int? EstimatedMinutes,
    DateOnly? ScheduledDate);
