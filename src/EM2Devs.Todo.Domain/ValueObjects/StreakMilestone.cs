namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a streak milestone reached at a specific day threshold.
/// Milestones are celebrated at key thresholds to motivate users.
/// Maps to: docs/features/progression/streaks.feature — "Streak milestone celebration"
/// </summary>
public sealed record StreakMilestone
{
    private static readonly IReadOnlyDictionary<int, string> _milestones = new Dictionary<int, string>
    {
        { 7, "One Week" },
        { 14, "Two Weeks" },
        { 30, "One Month" },
        { 60, "Two Months" },
        { 100, "The Century" },
        { 365, "The Full Year" },
    };

    public int Days { get; }
    public string Label { get; }

    private StreakMilestone(int days, string label)
    {
        Days = days;
        Label = label;
    }

    /// <summary>
    /// Returns the milestone for the given streak day count, or null if not a milestone.
    /// </summary>
    public static StreakMilestone? ForDays(int days)
    {
        return _milestones.TryGetValue(days, out string? label)
            ? new StreakMilestone(days, label)
            : null;
    }

    /// <summary>
    /// Returns all milestone thresholds in ascending order.
    /// </summary>
    public static IReadOnlyList<int> Thresholds => _milestones.Keys.Order().ToList().AsReadOnly();
}
