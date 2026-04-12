namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a level milestone reached at a specific level threshold.
/// Milestones are celebrated at key levels (10, 25, 50, 100).
/// Maps to: docs/features/progression/levelling.feature — "Level milestones are celebrated"
/// </summary>
public sealed record LevelMilestone
{
    private static readonly IReadOnlyDictionary<int, string> _milestones = new Dictionary<int, string>
    {
        { 10, "Double Digits" },
        { 25, "Quarter Century" },
        { 50, "Half Century" },
        { 100, "The Centurion" },
    };

    public int Level { get; }
    public string Label { get; }

    private LevelMilestone(int level, string label)
    {
        Level = level;
        Label = label;
    }

    /// <summary>
    /// Returns the milestone for the given level, or null if not a milestone level.
    /// </summary>
    public static LevelMilestone? ForLevel(int level)
    {
        return _milestones.TryGetValue(level, out string? label)
            ? new LevelMilestone(level, label)
            : null;
    }

    /// <summary>
    /// Returns all milestone level thresholds in ascending order.
    /// </summary>
    public static IReadOnlyList<int> Thresholds => _milestones.Keys.Order().ToList().AsReadOnly();
}
