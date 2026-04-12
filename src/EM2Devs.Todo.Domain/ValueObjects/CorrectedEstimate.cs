namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a corrected time estimate suggestion based on historical estimation bias.
/// Includes the original estimate, the suggested corrected estimate, the bias factor applied,
/// an explanation message, and whether the user accepted the suggestion.
/// </summary>
public sealed record CorrectedEstimate
{
    public TimeEstimate Original { get; }
    public TimeEstimate Suggested { get; }
    public double BiasFactorPercent { get; }
    public string Explanation { get; }
    public bool? Accepted { get; private init; }

    private CorrectedEstimate(TimeEstimate original, TimeEstimate suggested, double biasFactorPercent, string explanation, bool? accepted)
    {
        Original = original;
        Suggested = suggested;
        BiasFactorPercent = biasFactorPercent;
        Explanation = explanation;
        Accepted = accepted;
    }

    /// <summary>
    /// Creates a corrected estimate by applying the bias factor to the original estimate.
    /// </summary>
    /// <param name="original">The user's original time estimate.</param>
    /// <param name="biasFactorPercent">The detected bias percentage (positive = underestimation, negative = overestimation).</param>
    /// <param name="category">The task category for the explanation message.</param>
    public static CorrectedEstimate Create(TimeEstimate original, double biasFactorPercent, TaskCategory category)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(category);

        double correctionFactor = 1.0 + (biasFactorPercent / 100.0);
        int correctedMinutes = (int)Math.Round(original.Minutes * correctionFactor);

        correctedMinutes = Math.Max(correctedMinutes, 1);

        TimeEstimate suggested = TimeEstimate.FromMinutes(correctedMinutes);

        string direction = biasFactorPercent > 0 ? "longer" : "shorter";
        string explanation = $"Based on your history, {category.Value} tasks typically take {Math.Abs(biasFactorPercent):F0}% {direction} than estimated";

        return new CorrectedEstimate(original, suggested, biasFactorPercent, explanation, null);
    }

    /// <summary>
    /// Returns a new CorrectedEstimate marked as accepted by the user.
    /// </summary>
    public CorrectedEstimate Accept()
    {
        return this with { Accepted = true };
    }

    /// <summary>
    /// Returns a new CorrectedEstimate marked as dismissed by the user.
    /// </summary>
    public CorrectedEstimate Dismiss()
    {
        return this with { Accepted = false };
    }
}
