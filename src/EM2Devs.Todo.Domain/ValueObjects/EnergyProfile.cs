namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a user's weekly energy pattern, mapping each day of the week to a typical energy level.
/// Requires at least 7 check-ins to be considered sufficient data for pattern inference.
/// </summary>
public sealed class EnergyProfile : IEquatable<EnergyProfile>
{
    private const int MinimumDaysForSufficientData = 7;

    public const string InsufficientDataMessage =
        "We're still learning your energy patterns — check in daily for personalised suggestions after 14 days";

    private readonly IReadOnlyDictionary<DayOfWeek, EnergyLevel> _patterns;

    public bool HasSufficientData { get; }

    private EnergyProfile(IReadOnlyDictionary<DayOfWeek, EnergyLevel> patterns)
    {
        _patterns = patterns;
        HasSufficientData = patterns.Count >= MinimumDaysForSufficientData;
    }

    /// <summary>
    /// Creates an EnergyProfile from a dictionary of day-of-week to typical energy level mappings.
    /// </summary>
    public static EnergyProfile FromCheckIns(IDictionary<DayOfWeek, EnergyLevel> checkIns)
    {
        ArgumentNullException.ThrowIfNull(checkIns);
        return new EnergyProfile(new Dictionary<DayOfWeek, EnergyLevel>(checkIns));
    }

    /// <summary>
    /// Returns the typical energy level for the given day of the week.
    /// Defaults to Medium when insufficient data or the day has no pattern.
    /// </summary>
    public EnergyLevel GetTypicalEnergy(DayOfWeek day)
    {
        if (!HasSufficientData)
        {
            return EnergyLevel.Medium;
        }

        return _patterns.TryGetValue(day, out var level) ? level : EnergyLevel.Medium;
    }

    public bool Equals(EnergyProfile? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (_patterns.Count != other._patterns.Count)
        {
            return false;
        }

        foreach (var kvp in _patterns)
        {
            if (!other._patterns.TryGetValue(kvp.Key, out var otherValue) || kvp.Value != otherValue)
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as EnergyProfile);
    }

    public override int GetHashCode()
    {
        return _patterns.Aggregate(0, (hash, kvp) =>
            hash ^ HashCode.Combine(kvp.Key, kvp.Value));
    }
}
