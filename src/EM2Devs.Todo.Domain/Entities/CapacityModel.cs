using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// A personal capacity model that tracks per-day-of-week capacity in weighted task units.
/// Built from historical task completion data and updated gradually via weighted averages.
/// </summary>
public sealed class CapacityModel
{
    private readonly Dictionary<DayOfWeek, int> _capacityByDay;

    public IReadOnlyDictionary<DayOfWeek, int> CapacityByDay => _capacityByDay;

    private CapacityModel(Dictionary<DayOfWeek, int> capacityByDay)
    {
        _capacityByDay = capacityByDay;
    }

    /// <summary>
    /// Builds a capacity model from historical task completions grouped by day of week.
    /// Each entry maps a DayOfWeek to the average weighted capacity units completed on that day.
    /// </summary>
    public static CapacityModel BuildFromHistory(Dictionary<DayOfWeek, int> averageCapacityByDay)
    {
        ArgumentNullException.ThrowIfNull(averageCapacityByDay);

        Dictionary<DayOfWeek, int> capacity = new Dictionary<DayOfWeek, int>();
        foreach (KeyValuePair<DayOfWeek, int> entry in averageCapacityByDay)
        {
            if (entry.Value < 0)
            {
                throw new DomainException("Capacity cannot be negative.");
            }

            capacity[entry.Key] = entry.Value;
        }

        return new CapacityModel(capacity);
    }

    /// <summary>
    /// Returns the capacity for a specific day of the week, or 0 if no data exists.
    /// </summary>
    public int GetCapacity(DayOfWeek day)
    {
        return _capacityByDay.TryGetValue(day, out int capacity) ? capacity : 0;
    }

    /// <summary>
    /// Recalibrates capacity for a specific day using a weighted average.
    /// The adjustment is capped at a maximum change of maxAdjustment per recalibration.
    /// Uses a weighted average: (currentCapacity * historicalWeight + newObserved * recentWeight) / totalWeight.
    /// </summary>
    public void Recalibrate(DayOfWeek day, int newObservedCapacity, int historicalWeight, int recentWeight, int maxAdjustment)
    {
        if (newObservedCapacity < 0)
        {
            throw new DomainException("Observed capacity cannot be negative.");
        }

        if (historicalWeight <= 0)
        {
            throw new DomainException("Historical weight must be positive.");
        }

        if (recentWeight <= 0)
        {
            throw new DomainException("Recent weight must be positive.");
        }

        if (maxAdjustment <= 0)
        {
            throw new DomainException("Max adjustment must be positive.");
        }

        int currentCapacity = GetCapacity(day);
        int weightedAverage = (currentCapacity * historicalWeight + newObservedCapacity * recentWeight) / (historicalWeight + recentWeight);
        int delta = Math.Clamp(weightedAverage - currentCapacity, -maxAdjustment, maxAdjustment);

        _capacityByDay[day] = currentCapacity + delta;
    }

    /// <summary>
    /// Calculates the total weighted units for a set of tasks based on their difficulties.
    /// Tasks with no difficulty specified default to Normal.
    /// </summary>
    public static int CalculateScheduledUnits(IEnumerable<TaskDifficulty?> taskDifficulties)
    {
        ArgumentNullException.ThrowIfNull(taskDifficulties);

        int totalUnits = 0;
        foreach (TaskDifficulty? difficulty in taskDifficulties)
        {
            totalUnits += DifficultyWeight.For(difficulty ?? TaskDifficulty.Normal);
        }

        return totalUnits;
    }

    /// <summary>
    /// Checks whether scheduled tasks exceed capacity for a given day.
    /// </summary>
    public bool IsOvercommitted(DayOfWeek day, int scheduledUnits)
    {
        return scheduledUnits > GetCapacity(day);
    }

    /// <summary>
    /// Generates an overcommitment warning message if scheduled units exceed capacity.
    /// Returns null if within capacity.
    /// </summary>
    public OvercommitmentWarning? CheckOvercommitment(DayOfWeek day, int scheduledTaskCount, int scheduledUnits)
    {
        int capacity = GetCapacity(day);
        if (scheduledUnits <= capacity)
        {
            return null;
        }

        return OvercommitmentWarning.Create(day, capacity, scheduledTaskCount, scheduledUnits);
    }
}
