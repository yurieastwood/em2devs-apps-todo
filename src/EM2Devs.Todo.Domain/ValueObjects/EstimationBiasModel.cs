namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Tracks per-category estimation bias including average variance,
/// record count, and detected bias direction.
/// Immutable value object used by the estimation learning system.
/// </summary>
public sealed record EstimationBiasModel
{
    public TaskCategory Category { get; }
    public double AverageVariancePercent { get; }
    public int RecordCount { get; }
    public EstimationBiasType BiasType { get; }

    private EstimationBiasModel(TaskCategory category, double averageVariancePercent, int recordCount, EstimationBiasType biasType)
    {
        Category = category;
        AverageVariancePercent = averageVariancePercent;
        RecordCount = recordCount;
        BiasType = biasType;
    }

    /// <summary>
    /// Creates a bias model from computed statistics.
    /// Bias detection requires a minimum number of records and uses a configurable accuracy threshold.
    /// </summary>
    /// <param name="category">The task category.</param>
    /// <param name="averageVariancePercent">The average variance percentage across records.</param>
    /// <param name="recordCount">The number of estimation records analysed.</param>
    /// <param name="minimumRecords">Minimum records required to detect bias (default 5).</param>
    /// <param name="accuracyThreshold">Variance threshold within which estimates are considered accurate (default 30%).</param>
    public static EstimationBiasModel Create(
        TaskCategory category,
        double averageVariancePercent,
        int recordCount,
        int minimumRecords = 5,
        double accuracyThreshold = 30.0)
    {
        ArgumentNullException.ThrowIfNull(category);

        double rounded = Math.Round(averageVariancePercent, 1);
        EstimationBiasType biasType = DetectBias(rounded, recordCount, minimumRecords, accuracyThreshold);

        return new EstimationBiasModel(category, rounded, recordCount, biasType);
    }

    private static EstimationBiasType DetectBias(double averageVariance, int recordCount, int minimumRecords, double accuracyThreshold)
    {
        if (recordCount < minimumRecords)
        {
            return EstimationBiasType.None;
        }

        if (averageVariance > accuracyThreshold)
        {
            return EstimationBiasType.Underestimation;
        }

        if (averageVariance < -100.0)
        {
            return EstimationBiasType.DramaticOverestimation;
        }

        if (averageVariance + accuracyThreshold < 0)
        {
            return EstimationBiasType.Overestimation;
        }

        return EstimationBiasType.None;
    }
}
