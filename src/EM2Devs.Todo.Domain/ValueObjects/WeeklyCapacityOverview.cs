using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Summary of a user's average daily capacity for every day of the week.
/// Provides aggregate insights such as most and least productive days.
/// </summary>
public sealed record WeeklyCapacityOverview
{
    private readonly IReadOnlyDictionary<DayOfWeek, int> _capacityByDay;

    public IReadOnlyDictionary<DayOfWeek, int> CapacityByDay => _capacityByDay;
    public DayOfWeek MostProductiveDay { get; }
    public DayOfWeek LeastProductiveDay { get; }
    public int AverageDailyCapacity { get; }

    private WeeklyCapacityOverview(
        IReadOnlyDictionary<DayOfWeek, int> capacityByDay,
        DayOfWeek mostProductiveDay,
        DayOfWeek leastProductiveDay,
        int averageDailyCapacity)
    {
        _capacityByDay = capacityByDay;
        MostProductiveDay = mostProductiveDay;
        LeastProductiveDay = leastProductiveDay;
        AverageDailyCapacity = averageDailyCapacity;
    }

    public static WeeklyCapacityOverview From(IReadOnlyDictionary<DayOfWeek, int> capacityByDay)
    {
        ArgumentNullException.ThrowIfNull(capacityByDay);

        if (capacityByDay.Count != 7)
        {
            throw new DomainException("Weekly overview requires capacity for all 7 days.");
        }

        foreach (KeyValuePair<DayOfWeek, int> kvp in capacityByDay)
        {
            if (kvp.Value < 0)
            {
                throw new DomainException("Capacity cannot be negative.");
            }
        }

        Dictionary<DayOfWeek, int> snapshot = new Dictionary<DayOfWeek, int>(capacityByDay);

        DayOfWeek most = DayOfWeek.Sunday;
        DayOfWeek least = DayOfWeek.Sunday;
        int mostValue = int.MinValue;
        int leastValue = int.MaxValue;
        foreach (KeyValuePair<DayOfWeek, int> kvp in snapshot.OrderBy(kvp => kvp.Key))
        {
            if (kvp.Value > mostValue)
            {
                mostValue = kvp.Value;
                most = kvp.Key;
            }
            if (kvp.Value < leastValue)
            {
                leastValue = kvp.Value;
                least = kvp.Key;
            }
        }

        int average = snapshot.Values.Sum() / 7;

        return new WeeklyCapacityOverview(snapshot, most, least, average);
    }
}
