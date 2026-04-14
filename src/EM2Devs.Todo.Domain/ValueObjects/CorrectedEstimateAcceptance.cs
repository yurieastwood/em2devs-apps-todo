using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Records the outcome when a user accepts or dismisses a corrected estimate and later
/// completes the task. Used to reinforce or reduce the bias correction factor.
/// </summary>
public sealed record CorrectedEstimateAcceptance
{
    public TimeEstimate OriginalEstimate { get; }
    public TimeEstimate AcceptedEstimate { get; }
    public TimeEstimate ActualTime { get; }
    public bool WasAccepted { get; }

    private CorrectedEstimateAcceptance(
        TimeEstimate originalEstimate,
        TimeEstimate acceptedEstimate,
        TimeEstimate actualTime,
        bool wasAccepted)
    {
        OriginalEstimate = originalEstimate;
        AcceptedEstimate = acceptedEstimate;
        ActualTime = actualTime;
        WasAccepted = wasAccepted;
    }

    public static CorrectedEstimateAcceptance Create(
        TimeEstimate originalEstimate,
        TimeEstimate acceptedEstimate,
        TimeEstimate actualTime,
        bool wasAccepted)
    {
        ArgumentNullException.ThrowIfNull(originalEstimate);
        ArgumentNullException.ThrowIfNull(acceptedEstimate);
        ArgumentNullException.ThrowIfNull(actualTime);

        if (!wasAccepted && acceptedEstimate != originalEstimate)
        {
            throw new DomainException("When the suggestion is dismissed, the accepted estimate must equal the original.");
        }

        return new CorrectedEstimateAcceptance(originalEstimate, acceptedEstimate, actualTime, wasAccepted);
    }

    private static int AbsDiff(TimeEstimate a, TimeEstimate b) => Math.Abs(a.Minutes - b.Minutes);

    /// <summary>
    /// Indicates that the original estimate was closer to actual than the accepted corrected estimate.
    /// </summary>
    public bool OriginalWasMoreAccurate => AbsDiff(OriginalEstimate, ActualTime) < AbsDiff(AcceptedEstimate, ActualTime);

    /// <summary>
    /// Computes the bias correction adjustment to apply. A single instance only contributes a
    /// small fractional adjustment to avoid over-correction.
    /// </summary>
    public double ComputeBiasAdjustment(double currentBiasFactorPercent, double singleInstanceDampening = 0.1)
    {
        if (singleInstanceDampening <= 0 || singleInstanceDampening > 1)
        {
            throw new DomainException("Single-instance dampening must be within (0, 1].");
        }

        if (!WasAccepted || !OriginalWasMoreAccurate)
        {
            return 0.0;
        }

        // Reduce the bias factor by a small fraction toward zero.
        double adjustment = -currentBiasFactorPercent * singleInstanceDampening;
        return Math.Round(adjustment, 3);
    }
}
