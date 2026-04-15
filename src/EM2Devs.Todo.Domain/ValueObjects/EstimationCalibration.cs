namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Calibration state for the time-estimation learning ritual. Describes whether
/// the system has enough samples to calibrate future estimates and, if so, what
/// bias factor to apply (multiplier against original estimated minutes).
/// </summary>
public enum EstimationCalibrationState
{
    /// <summary>Not enough completed-with-actual samples to calibrate.</summary>
    NotEnoughData = 0,

    /// <summary>Sufficient samples exist; a bias factor has been computed.</summary>
    Calibrated = 1,
}

/// <summary>
/// Read-side projection describing the authenticated user's estimation calibration.
/// A neutral <see cref="BiasFactor"/> of <c>1.0</c> is returned when the state is
/// <see cref="EstimationCalibrationState.NotEnoughData"/>.
/// </summary>
public sealed record EstimationCalibration
{
    /// <summary>Lower clamp on the detected bias factor (50% of original estimate).</summary>
    public const double MinBiasFactor = 0.5;

    /// <summary>Upper clamp on the detected bias factor (200% of original estimate).</summary>
    public const double MaxBiasFactor = 2.0;

    public double BiasFactor { get; }
    public int SampleSize { get; }
    public EstimationCalibrationState State { get; }

    private EstimationCalibration(double biasFactor, int sampleSize, EstimationCalibrationState state)
    {
        BiasFactor = biasFactor;
        SampleSize = sampleSize;
        State = state;
    }

    /// <summary>
    /// Creates a "not enough data" calibration with a neutral bias factor of 1.0.
    /// Callers pass the non-negative count of samples currently available.
    /// </summary>
    public static EstimationCalibration NotEnoughData(int sampleSize)
    {
        return new EstimationCalibration(1.0, sampleSize, EstimationCalibrationState.NotEnoughData);
    }

    /// <summary>
    /// Creates a calibrated projection. The bias factor is clamped between
    /// <see cref="MinBiasFactor"/> and <see cref="MaxBiasFactor"/> to protect
    /// against outlier-dominated samples.
    /// </summary>
    public static EstimationCalibration Calibrated(double biasFactor, int sampleSize)
    {
        double clamped = Math.Clamp(biasFactor, MinBiasFactor, MaxBiasFactor);
        double rounded = Math.Round(clamped, 2);
        return new EstimationCalibration(rounded, sampleSize, EstimationCalibrationState.Calibrated);
    }

    /// <summary>
    /// Applies the bias factor to an estimate and returns the calibrated minutes,
    /// rounded to the nearest whole minute. Returns <c>null</c> when the state is
    /// <see cref="EstimationCalibrationState.NotEnoughData"/>, signalling the caller
    /// should fall back to the raw estimate.
    /// </summary>
    public int? ApplyTo(int estimatedMinutes)
    {
        if (State == EstimationCalibrationState.NotEnoughData)
        {
            return null;
        }

        // With BiasFactor clamped to [0.5, 2.0] and estimatedMinutes >= 1, the product
        // is always >= 0.5 which rounds up to 1 — the output is always a positive
        // number of minutes.
        double calibrated = estimatedMinutes * BiasFactor;
        return (int)Math.Round(calibrated, MidpointRounding.AwayFromZero);
    }
}
