using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that computes a user's overall estimation bias factor
/// from the history of <c>(estimated, actual)</c> pairs attached to tasks.
///
/// The bias factor is the median of <c>actual / estimated</c> across all samples
/// — median rather than mean to resist outlier-domination. A value &gt; 1.0 means
/// the user underestimates; &lt; 1.0 means the user overestimates. The factor is
/// clamped in <see cref="EstimationCalibration"/> to [0.5, 2.0].
/// </summary>
public static class TimeEstimationCalibrator
{
    /// <summary>Default minimum number of completed-with-actual samples required to calibrate.</summary>
    public const int DefaultMinimumSamples = 3;

    /// <summary>
    /// Computes a calibration projection from a collection of tasks.
    /// Only tasks with both <see cref="TodoTask.EstimatedTime"/> and
    /// <see cref="TodoTask.ActualTimeRecord"/> contribute to the sample.
    /// </summary>
    public static EstimationCalibration Calibrate(
        IReadOnlyList<TodoTask> tasks,
        int minimumSamples = DefaultMinimumSamples)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        List<double> ratios = [];
        for (int i = 0; i < tasks.Count; i++)
        {
            TodoTask task = tasks[i];
            EstimationRecord? record = task.ActualTimeRecord;
            if (record is null)
            {
                continue;
            }

            // Both Estimated and Actual are TimeEstimate value objects, which enforce
            // Minutes > 0 at construction — so no zero-division guard is needed here.
            int estimated = record.Estimated.Minutes;
            int actual = record.Actual.Minutes;
            ratios.Add((double)actual / estimated);
        }

        int sampleSize = ratios.Count;
        if (sampleSize < minimumSamples)
        {
            return EstimationCalibration.NotEnoughData(sampleSize);
        }

        double median = Median(ratios);
        return EstimationCalibration.Calibrated(median, sampleSize);
    }

    private static double Median(List<double> values)
    {
        values.Sort();
        int count = values.Count;
        int mid = count / 2;
        if (count % 2 == 1)
        {
            return values[mid];
        }

        return (values[mid - 1] + values[mid]) / 2.0;
    }
}
