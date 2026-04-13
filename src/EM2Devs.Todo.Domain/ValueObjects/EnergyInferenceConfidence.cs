using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents the confidence of an inferred energy level based on the number of data points available.
/// Confidence is bounded between 0.0 and 1.0 and rises as more check-ins are accumulated.
/// </summary>
public sealed record EnergyInferenceConfidence
{
    /// <summary>
    /// Minimum data points considered to produce any meaningful inference.
    /// </summary>
    public const int MinimumDataPoints = 14;

    /// <summary>
    /// Data points that represent full confidence (1.0).
    /// </summary>
    public const int HighConfidenceDataPoints = 60;

    /// <summary>
    /// Threshold above which confidence is classified as "High".
    /// </summary>
    public const double HighConfidenceThreshold = 0.75;

    /// <summary>
    /// Threshold above which confidence is classified as "Moderate".
    /// </summary>
    public const double ModerateConfidenceThreshold = 0.25;

    public double Score { get; }
    public int DataPoints { get; }

    private EnergyInferenceConfidence(double score, int dataPoints)
    {
        Score = score;
        DataPoints = dataPoints;
    }

    public static EnergyInferenceConfidence FromDataPoints(int dataPoints)
    {
        if (dataPoints < 0)
        {
            throw new DomainException("Data points cannot be negative.");
        }

        double raw = (double)(dataPoints - MinimumDataPoints)
            / (HighConfidenceDataPoints - MinimumDataPoints);
        double clamped = Math.Clamp(raw, 0.0, 1.0);
        return new EnergyInferenceConfidence(Math.Round(clamped, 3), dataPoints);
    }

    /// <summary>
    /// Creates a confidence value with an explicit score and supporting data-point count.
    /// </summary>
    public static EnergyInferenceConfidence FromScore(double score, int dataPoints)
    {
        if (dataPoints < 0)
        {
            throw new DomainException("Data points cannot be negative.");
        }

        if (score < 0.0 || score > 1.0)
        {
            throw new DomainException("Confidence score must be between 0 and 1.");
        }

        return new EnergyInferenceConfidence(score, dataPoints);
    }

    public bool IsHigh => Score >= HighConfidenceThreshold;
    public bool IsModerate => Score >= ModerateConfidenceThreshold && Score < HighConfidenceThreshold;
    public bool IsLow => Score < ModerateConfidenceThreshold;
}
