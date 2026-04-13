using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Detects mid-day energy shifts and recommends task reordering when a dip is expected.
/// </summary>
public static class EnergyShiftDetector
{
    /// <summary>
    /// Evaluates whether the user is expected to experience an energy dip at the current hour,
    /// given a typical-dip hour and the morning energy level.
    /// </summary>
    /// <param name="morningEnergy">Morning energy level reported by the user.</param>
    /// <param name="currentHour">Current hour of day (0-23).</param>
    /// <param name="typicalDipHour">The hour at which the user typically dips in energy.</param>
    public static EnergyShiftRecommendation Evaluate(
        EnergyLevel morningEnergy,
        int currentHour,
        int typicalDipHour)
    {
        if (currentHour < 0 || currentHour > 23)
        {
            throw new Exceptions.DomainException("Current hour must be between 0 and 23.");
        }

        if (typicalDipHour < 0 || typicalDipHour > 23)
        {
            throw new Exceptions.DomainException("Typical dip hour must be between 0 and 23.");
        }

        bool dipExpected = currentHour >= typicalDipHour && morningEnergy >= EnergyLevel.High;

        if (!dipExpected)
        {
            return EnergyShiftRecommendation.NoShift();
        }

        return EnergyShiftRecommendation.SuggestLighterTasks(
            "Energy usually dips around now — lighter tasks might be a good fit");
    }
}
