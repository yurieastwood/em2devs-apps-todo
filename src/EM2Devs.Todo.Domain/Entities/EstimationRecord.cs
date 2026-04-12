using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Records estimated vs actual time for a completed task.
/// Calculates variance percentage to feed the estimation learning model.
/// </summary>
public sealed class EstimationRecord
{
    public EstimationRecordId Id { get; }
    public TimeEstimate Estimated { get; }
    public TimeEstimate Actual { get; }
    public double VariancePercent { get; }
    public TaskCategory? Category { get; }

    private EstimationRecord(EstimationRecordId id, TimeEstimate estimated, TimeEstimate actual, double variancePercent, TaskCategory? category)
    {
        Id = id;
        Estimated = estimated;
        Actual = actual;
        VariancePercent = variancePercent;
        Category = category;
    }

    public static EstimationRecord Create(TimeEstimate estimated, TimeEstimate actual, TaskCategory? category = null)
    {
        ArgumentNullException.ThrowIfNull(estimated);
        ArgumentNullException.ThrowIfNull(actual);

        double variance = (double)(actual.Minutes - estimated.Minutes) / estimated.Minutes * 100.0;
        double rounded = Math.Round(variance, 1);

        return new EstimationRecord(EstimationRecordId.New(), estimated, actual, rounded, category);
    }
}
