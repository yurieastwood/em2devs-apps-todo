using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Per-category accuracy statistics derived from estimation records.
/// </summary>
public sealed record CategoryAccuracyStats
{
    public TaskCategory Category { get; }
    public int RecordCount { get; }
    public double AverageVariancePercent { get; }
    public double AccuracyPercent { get; }

    public CategoryAccuracyStats(TaskCategory category, int recordCount, double averageVariancePercent, double accuracyPercent)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (recordCount <= 0)
        {
            throw new DomainException("Record count must be positive.");
        }

        Category = category;
        RecordCount = recordCount;
        AverageVariancePercent = averageVariancePercent;
        AccuracyPercent = accuracyPercent;
    }
}

/// <summary>
/// Aggregate estimation-accuracy dashboard providing an overall accuracy percentage,
/// per-category breakdown, and a time-ordered trend of accuracy values.
/// </summary>
public sealed record EstimationDashboard
{
    private readonly IReadOnlyList<CategoryAccuracyStats> _perCategory;
    private readonly IReadOnlyList<double> _accuracyTrend;

    public double OverallAccuracyPercent { get; }
    public IReadOnlyList<CategoryAccuracyStats> PerCategory => _perCategory;
    public IReadOnlyList<double> AccuracyTrend => _accuracyTrend;

    private EstimationDashboard(
        double overallAccuracyPercent,
        IReadOnlyList<CategoryAccuracyStats> perCategory,
        IReadOnlyList<double> accuracyTrend)
    {
        OverallAccuracyPercent = overallAccuracyPercent;
        _perCategory = perCategory;
        _accuracyTrend = accuracyTrend;
    }

    /// <summary>
    /// Builds the dashboard from completed estimation records, grouped by category,
    /// plus an ordered list of snapshots for trend display.
    /// </summary>
    public static EstimationDashboard Build(
        IReadOnlyList<EstimationRecord> records,
        IReadOnlyList<double> accuracyTrend)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(accuracyTrend);

        if (records.Count == 0)
        {
            throw new DomainException("At least one estimation record is required to build a dashboard.");
        }

        double overallVariance = records.Average(r => Math.Abs(r.VariancePercent));
        double overallAccuracy = Math.Round(Math.Max(0.0, 100.0 - overallVariance), 2);

        List<CategoryAccuracyStats> perCategory = new List<CategoryAccuracyStats>();
        foreach (IGrouping<TaskCategory, EstimationRecord> group in records
            .Where(r => r.Category is not null)
            .GroupBy(r => r.Category!))
        {
            int count = group.Count();
            double avgVariance = Math.Round(group.Average(r => r.VariancePercent), 2);
            double accuracy = Math.Round(Math.Max(0.0, 100.0 - Math.Abs(avgVariance)), 2);
            perCategory.Add(new CategoryAccuracyStats(group.Key, count, avgVariance, accuracy));
        }

        return new EstimationDashboard(overallAccuracy, perCategory, new List<double>(accuracyTrend));
    }
}
