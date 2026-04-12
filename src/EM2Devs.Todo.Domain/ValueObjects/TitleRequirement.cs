namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Defines the sustained-behaviour requirement for earning a title.
/// A title requires both a minimum action count and a minimum number of distinct days
/// to ensure the behaviour is sustained over time, not achieved in a single burst.
/// </summary>
public sealed record TitleRequirement
{
    public TitleType TitleType { get; }

    /// <summary>Minimum number of qualifying actions required.</summary>
    public int RequiredCount { get; }

    /// <summary>Minimum number of distinct days the actions must span.</summary>
    public int RequiredDistinctDays { get; }

    /// <summary>Human-readable label for the action being counted (e.g., "qualifying actions", "days of consistent completions").</summary>
    public string ActionLabel { get; }

    private TitleRequirement(TitleType titleType, int requiredCount, int requiredDistinctDays, string actionLabel)
    {
        TitleType = titleType;
        RequiredCount = requiredCount;
        RequiredDistinctDays = requiredDistinctDays;
        ActionLabel = actionLabel;
    }

    /// <summary>
    /// Returns the requirement definition for a given title type.
    /// </summary>
    public static TitleRequirement For(TitleType titleType) =>
        titleType switch
        {
            TitleType.EarlyBird => new TitleRequirement(titleType, 50, 28, "qualifying actions"),
            TitleType.MorningArchitect => new TitleRequirement(titleType, 30, 42, "qualifying actions"),
            TitleType.NightOwl => new TitleRequirement(titleType, 50, 28, "qualifying actions"),
            TitleType.MarathonBuilder => new TitleRequirement(titleType, 60, 60, "days of consistent completions"),
            TitleType.BossSlayer => new TitleRequirement(titleType, 10, 1, "qualifying actions"),
            TitleType.StreakMaster => new TitleRequirement(titleType, 30, 30, "days of consistent completions"),
            TitleType.QuestCloser => new TitleRequirement(titleType, 25, 1, "qualifying actions"),
            TitleType.ConsistentPlanner => new TitleRequirement(titleType, 12, 12, "qualifying actions"),
            TitleType.TeamAnchor => new TitleRequirement(titleType, 8, 8, "qualifying actions"),
            _ => throw new ArgumentOutOfRangeException(
                nameof(titleType), titleType, "Unknown title type.")
        };
}
