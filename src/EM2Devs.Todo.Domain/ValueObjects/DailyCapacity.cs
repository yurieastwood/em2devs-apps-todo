using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a learned daily task capacity measured in task units.
/// Normal tasks = 1 unit, Hard tasks = 2 units (ADR-0002).
/// </summary>
public sealed record DailyCapacity
{
    public int TaskUnits { get; }

    private DailyCapacity(int taskUnits)
    {
        TaskUnits = taskUnits;
    }

    public static DailyCapacity FromTaskUnits(int taskUnits)
    {
        if (taskUnits < 0)
        {
            throw new DomainException("Capacity cannot be negative.");
        }

        return new DailyCapacity(taskUnits);
    }
}
