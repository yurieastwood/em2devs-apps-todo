using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that infers energy levels from historical patterns.
/// Supports inference from time-of-day windows and day-of-week profiles.
/// </summary>
public static class EnergyPatternDetector
{
    /// <summary>
    /// Infers the typical energy level at a specific hour-of-day based on a per-hour pattern.
    /// Defaults to Medium when the hour has no recorded pattern.
    /// </summary>
    public static EnergyLevel InferByHour(IReadOnlyDictionary<int, EnergyLevel> hourlyPattern, int hourOfDay)
    {
        ArgumentNullException.ThrowIfNull(hourlyPattern);

        if (hourOfDay < 0 || hourOfDay > 23)
        {
            throw new Exceptions.DomainException("Hour of day must be between 0 and 23.");
        }

        return hourlyPattern.TryGetValue(hourOfDay, out EnergyLevel level) ? level : EnergyLevel.Medium;
    }

    /// <summary>
    /// Builds a per-hour pattern from completion data: hours in which hard tasks are most often
    /// completed map to High energy; hours with easier/routine completion map to Low.
    /// </summary>
    public static IReadOnlyDictionary<int, EnergyLevel> BuildHourlyPattern(
        IReadOnlyDictionary<int, EnergyLevel> observedByHour)
    {
        ArgumentNullException.ThrowIfNull(observedByHour);

        Dictionary<int, EnergyLevel> pattern = new Dictionary<int, EnergyLevel>();
        foreach (KeyValuePair<int, EnergyLevel> kvp in observedByHour)
        {
            if (kvp.Key < 0 || kvp.Key > 23)
            {
                throw new Exceptions.DomainException("Hour of day must be between 0 and 23.");
            }
            pattern[kvp.Key] = kvp.Value;
        }

        return pattern;
    }
}
