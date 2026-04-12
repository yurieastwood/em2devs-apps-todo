namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A quarterly season with themed content, quest line, and date range.
/// Seasons run for a fixed period and cannot overlap.
/// </summary>
public sealed record Season
{
    public string Name { get; }
    public string Theme { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public Season(string name, string theme, DateOnly startDate, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new Exceptions.DomainException("Season name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(theme))
        {
            throw new Exceptions.DomainException("Season theme cannot be empty.");
        }

        if (endDate <= startDate)
        {
            throw new Exceptions.DomainException("Season end date must be after start date.");
        }

        Name = name;
        Theme = theme;
        StartDate = startDate;
        EndDate = endDate;
    }

    public bool IsActive(DateOnly today) =>
        today >= StartDate && today <= EndDate;

    public int DaysRemaining(DateOnly today)
    {
        if (!IsActive(today))
        {
            return 0;
        }

        return EndDate.DayNumber - today.DayNumber;
    }

    public bool HasEnded(DateOnly today) =>
        today > EndDate;
}
