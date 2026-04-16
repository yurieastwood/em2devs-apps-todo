using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Infrastructure.Calendar;

public sealed class NullCalendarService : ICalendarService
{
    public Task<IReadOnlyList<CalendarBlock>> GetTodayBlocksAsync(DateOnly calendarDate, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<CalendarBlock>>([]);
}
