namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Pure domain calculation: suggests adjusting a task's difficulty
/// when actual completion time deviates significantly from the estimate.
/// Maps to: experience-points.feature — "Difficulty rating auto-adjusts for repeated identical tasks"
/// </summary>
public sealed record DifficultyAdjustSuggestion
{
    /// <summary>
    /// Variance threshold (exclusive) beyond which a suggestion is made.
    /// A task completed more than 50% faster or slower triggers a suggestion.
    /// </summary>
    private const double VarianceThreshold = 50.0;

    public TaskDifficulty SuggestedDifficulty { get; }
    public string Reason { get; }

    private DifficultyAdjustSuggestion(TaskDifficulty suggestedDifficulty, string reason)
    {
        SuggestedDifficulty = suggestedDifficulty;
        Reason = reason;
    }

    /// <summary>
    /// Evaluates whether a difficulty adjustment should be suggested based on
    /// estimated vs actual completion time. Returns null if no adjustment is warranted.
    /// </summary>
    public static DifficultyAdjustSuggestion? Evaluate(
        TaskDifficulty currentDifficulty,
        TimeEstimate estimated,
        TimeEstimate actual)
    {
        ArgumentNullException.ThrowIfNull(estimated);
        ArgumentNullException.ThrowIfNull(actual);

        double variancePercent = (double)(actual.Minutes - estimated.Minutes) / estimated.Minutes * 100.0;

        if (variancePercent < -VarianceThreshold)
        {
            // Completed significantly faster — suggest lower difficulty
            TaskDifficulty? lower = LowerDifficulty(currentDifficulty);
            if (lower is null)
            {
                return null;
            }

            return new DifficultyAdjustSuggestion(
                lower.Value,
                $"Task was completed significantly faster than estimated ({actual.Minutes}m vs {estimated.Minutes}m). Consider lowering the difficulty.");
        }

        if (variancePercent > VarianceThreshold)
        {
            // Completed significantly slower — suggest higher difficulty
            TaskDifficulty? higher = HigherDifficulty(currentDifficulty);
            if (higher is null)
            {
                return null;
            }

            return new DifficultyAdjustSuggestion(
                higher.Value,
                $"Task took significantly slower than estimated ({actual.Minutes}m vs {estimated.Minutes}m). Consider raising the difficulty.");
        }

        return null;
    }

    private static TaskDifficulty? LowerDifficulty(TaskDifficulty current) => current switch
    {
        TaskDifficulty.Trivial => null,
        TaskDifficulty.Easy => TaskDifficulty.Trivial,
        TaskDifficulty.Normal => TaskDifficulty.Easy,
        TaskDifficulty.Hard => TaskDifficulty.Normal,
        TaskDifficulty.Epic => TaskDifficulty.Hard,
        _ => null
    };

    private static TaskDifficulty? HigherDifficulty(TaskDifficulty current) => current switch
    {
        TaskDifficulty.Trivial => TaskDifficulty.Easy,
        TaskDifficulty.Easy => TaskDifficulty.Normal,
        TaskDifficulty.Normal => TaskDifficulty.Hard,
        TaskDifficulty.Hard => TaskDifficulty.Epic,
        TaskDifficulty.Epic => null,
        _ => null
    };
}
