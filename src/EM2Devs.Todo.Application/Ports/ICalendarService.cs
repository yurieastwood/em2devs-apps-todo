namespace EM2Devs.Todo.Application.Ports;

public sealed record CalendarBlock(TimeOnly Start, TimeOnly End)
{
    public int DurationMinutes => (int)(End - Start).TotalMinutes;
}

public interface ICalendarService
{
    Task<IReadOnlyList<CalendarBlock>> GetTodayBlocksAsync(DateOnly calendarDate, CancellationToken ct = default);
}
