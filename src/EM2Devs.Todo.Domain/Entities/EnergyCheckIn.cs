using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class EnergyCheckIn
{
    public EnergyCheckInId Id { get; }
    public EnergyLevel Level { get; }
    public DateTimeOffset RecordedAt { get; }

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
}
