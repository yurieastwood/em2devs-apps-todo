using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Direction of a capacity trend over a recent time window.
/// </summary>
public enum CapacityTrend
{
    Rising,
    Falling,
    Stable
}

/// <summary>
/// Provides a trend insight over a window of daily capacity observations.
/// Used to inform weekly planning decisions (e.g., front-load on high-capacity days).
/// </summary>
public sealed record CapacityInsight
{
    /// <summary>
    /// Absolute change in average capacity required to classify the trend as rising or falling.
    /// Differences within this band are considered Stable.
    /// </summary>
    public const double StableBand = 0.5;

    public CapacityTrend Trend { get; }
    public double RecentAverage { get; }
    public double PreviousAverage { get; }

    private CapacityInsight(CapacityTrend trend, double recentAverage, double previousAverage)
    {
        Trend = trend;
        RecentAverage = recentAverage;
        PreviousAverage = previousAverage;
    }

    /// <summary>
    /// Evaluates the trend by comparing the average of the recent window against the previous window.
    /// </summary>
    public static CapacityInsight Evaluate(IReadOnlyList<int> recent, IReadOnlyList<int> previous)
    {
        ArgumentNullException.ThrowIfNull(recent);
        ArgumentNullException.ThrowIfNull(previous);

        if (recent.Count == 0 || previous.Count == 0)
        {
            throw new DomainException("Both recent and previous windows must contain data.");
        }

        double recentAvg = recent.Average();
        double previousAvg = previous.Average();
        double delta = recentAvg - previousAvg;

        CapacityTrend trend;
        if (delta >= StableBand)
        {
            trend = CapacityTrend.Rising;
        }
        else if (delta <= -StableBand)
        {
            trend = CapacityTrend.Falling;
        }
        else
        {
            trend = CapacityTrend.Stable;
        }

        return new CapacityInsight(trend, Math.Round(recentAvg, 2), Math.Round(previousAvg, 2));
    }
}
