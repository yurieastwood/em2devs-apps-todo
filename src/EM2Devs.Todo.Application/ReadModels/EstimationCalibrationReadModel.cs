namespace EM2Devs.Todo.Application.ReadModels;

/// <summary>
/// Read model surfaced by <c>GET /api/profile/estimation-bias</c>. Describes the
/// authenticated user's estimation calibration: the bias factor (multiplier applied
/// to raw estimates), the sample size that produced it, and the calibration state.
/// </summary>
public sealed record EstimationCalibrationReadModel(
    double BiasFactor,
    int SampleSize,
    string CalibrationState);
