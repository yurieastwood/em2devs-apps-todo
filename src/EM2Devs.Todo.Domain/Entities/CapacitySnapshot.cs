using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// Records learned daily capacity for a specific day of the week.
/// Used for overcommitment detection and planning recommendations.
/// </summary>
public sealed class CapacitySnapshot
{
    public CapacitySnapshotId Id { get; }
    public DailyCapacity Capacity { get; }
    public DayOfWeek DayOfWeek { get; }

    private CapacitySnapshot(CapacitySnapshotId id, DailyCapacity capacity, DayOfWeek dayOfWeek)
    {
        Id = id;
        Capacity = capacity;
        DayOfWeek = dayOfWeek;
    }

    public static CapacitySnapshot Create(DailyCapacity capacity, DayOfWeek dayOfWeek)
    {
        return new CapacitySnapshot(CapacitySnapshotId.New(), capacity, dayOfWeek);
    }

    public bool IsOvercommitted(int scheduledUnits)
    {
        return scheduledUnits > Capacity.TaskUnits;
    }
}
