using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that evaluates estimation bias from a collection of estimation records.
/// Detects systematic under- or overestimation patterns per task category.
/// No infrastructure dependencies — all decisions are based on estimation data.
/// </summary>
public static class EstimationBiasDetector
{
    private const int DefaultMinimumRecords = 5;
    private const double DefaultAccuracyThreshold = 30.0;

    /// <summary>
    /// Analyses estimation records for a specific category and returns the detected bias model.
    /// </summary>
    /// <param name="records">The estimation records to analyse. Must all belong to the same category.</param>
    /// <param name="category">The task category being analysed.</param>
    /// <param name="minimumRecords">Minimum records required to detect bias.</param>
    /// <param name="accuracyThreshold">Variance threshold within which estimates are considered accurate.</param>
    public static EstimationBiasModel Analyse(
        IReadOnlyList<EstimationRecord> records,
        TaskCategory category,
        int minimumRecords = DefaultMinimumRecords,
        double accuracyThreshold = DefaultAccuracyThreshold)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(category);

        string categoryValue = category.Value;
        List<EstimationRecord> categoryRecords = records
            .Where(r => r.Category is not null && r.Category.Value == categoryValue)
            .ToList();

        if (categoryRecords.Count == 0)
        {
            return EstimationBiasModel.Create(category, 0.0, 0, minimumRecords, accuracyThreshold);
        }

        double averageVariance = categoryRecords.Average(r => r.VariancePercent);

        return EstimationBiasModel.Create(category, averageVariance, categoryRecords.Count, minimumRecords, accuracyThreshold);
    }

    /// <summary>
    /// Suggests a corrected estimate for a new task based on the detected bias for its category.
    /// Returns null if no significant bias is detected.
    /// </summary>
    /// <param name="original">The user's original time estimate.</param>
    /// <param name="biasModel">The detected bias model for the task's category.</param>
    public static CorrectedEstimate? SuggestCorrectedEstimate(TimeEstimate original, EstimationBiasModel biasModel)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(biasModel);

        if (biasModel.BiasType == EstimationBiasType.None)
        {
            return null;
        }

        return CorrectedEstimate.Create(original, biasModel.AverageVariancePercent, biasModel.Category);
    }

    /// <summary>
    /// Determines whether a task completion requires actual time recording.
    /// Only tasks that had an estimate require actual time tracking.
    /// </summary>
    /// <param name="hasEstimate">Whether the task had a time estimate.</param>
    public static bool RequiresActualTimeRecording(bool hasEstimate)
    {
        return hasEstimate;
    }

    /// <summary>
    /// Evaluates whether estimation accuracy has improved by comparing
    /// the variance of earlier records against more recent records.
    /// </summary>
    /// <param name="records">All estimation records for a category, ordered by creation.</param>
    /// <param name="category">The task category being analysed.</param>
    public static bool HasAccuracyImproved(IReadOnlyList<EstimationRecord> records, TaskCategory category)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(category);

        List<EstimationRecord> categoryRecords = records
            .Where(r => r.Category is not null && r.Category == category)
            .ToList();

        if (categoryRecords.Count < 4)
        {
            return false;
        }

        int midpoint = categoryRecords.Count / 2;
        List<EstimationRecord> earlyRecords = categoryRecords.Take(midpoint).ToList();
        List<EstimationRecord> recentRecords = categoryRecords.Skip(midpoint).ToList();

        double earlyAvgAbsVariance = earlyRecords.Average(r => Math.Abs(r.VariancePercent));
        double recentAvgAbsVariance = recentRecords.Average(r => Math.Abs(r.VariancePercent));

        return recentAvgAbsVariance < earlyAvgAbsVariance;
    }
}
