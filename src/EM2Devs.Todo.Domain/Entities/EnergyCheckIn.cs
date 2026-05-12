using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class EnergyCheckIn
{
    /// <summary>
    /// Minimum number of check-ins required before the system uses inferred patterns
    /// instead of defaults.
    /// </summary>
    public const int MinimumCheckInsForPattern = 7;

    public EnergyCheckInId Id { get; }
    public EnergyLevel Level { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }

    /// <summary>
    /// The energy level before the most recent update, or null if never updated.
    /// Tracked for rapid fluctuation pattern analysis.
    /// </summary>
    public EnergyLevel? PreviousLevel { get; private set; }

    /// <summary>
    /// Indicates whether the energy level was updated after initial creation,
    /// signalling a rapid fluctuation for future pattern analysis.
    /// </summary>
    public bool HasFluctuated { get; private set; }

    private EnergyCheckIn(EnergyCheckInId id, EnergyLevel level, DateTimeOffset recordedAt)
    {
        Id = id;
        Level = level;
        RecordedAt = recordedAt;
    }

    public static EnergyCheckIn Create(EnergyLevel level, DateTimeOffset recordedAt)
    {
        return new EnergyCheckIn(EnergyCheckInId.New(), level, recordedAt);
    }

    public static EnergyCheckIn CreateDefault(DateTimeOffset recordedAt)
    {
        return new EnergyCheckIn(EnergyCheckInId.New(), EnergyLevel.Medium, recordedAt);
    }

    /// <summary>
    /// Rebuilds an energy check-in from a persisted snapshot.
    /// </summary>
    public static EnergyCheckIn Reconstitute(
        EnergyCheckInId id,
        EnergyLevel level,
        DateTimeOffset recordedAt,
        EnergyLevel? previousLevel,
        bool hasFluctuated)
    {
        ArgumentNullException.ThrowIfNull(id);

        return new EnergyCheckIn(id, level, recordedAt)
        {
            PreviousLevel = previousLevel,
            HasFluctuated = hasFluctuated,
        };
    }

    /// <summary>
    /// Updates the energy level for a mid-day re-check-in.
    /// Records the previous level and marks the check-in as having fluctuated.
    /// </summary>
    public void UpdateLevel(EnergyLevel newLevel, DateTimeOffset updatedAt)
    {
        PreviousLevel = Level;
        Level = newLevel;
        RecordedAt = updatedAt;
        HasFluctuated = true;
    }
}
