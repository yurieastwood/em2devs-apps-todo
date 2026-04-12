namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a warning that scheduled tasks exceed the user's typical capacity for a day.
/// </summary>
public sealed record OvercommitmentWarning
{
    public DayOfWeek Day { get; }
    public int TypicalCapacityUnits { get; }
    public int ScheduledTaskCount { get; }
    public int ScheduledUnits { get; }
    public string Message { get; }

    private OvercommitmentWarning(DayOfWeek day, int typicalCapacityUnits, int scheduledTaskCount, int scheduledUnits, string message)
    {
        Day = day;
        TypicalCapacityUnits = typicalCapacityUnits;
        ScheduledTaskCount = scheduledTaskCount;
        ScheduledUnits = scheduledUnits;
        Message = message;
    }

    public static OvercommitmentWarning Create(DayOfWeek day, int typicalCapacityUnits, int scheduledTaskCount, int scheduledUnits)
    {
        string message = $"You typically complete {typicalCapacityUnits} capacity units on {day}s. " +
                      $"You have {scheduledTaskCount} tasks ({scheduledUnits} units) scheduled. Consider reprioritising.";

        return new OvercommitmentWarning(day, typicalCapacityUnits, scheduledTaskCount, scheduledUnits, message);
    }
}
