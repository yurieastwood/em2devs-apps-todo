using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface ITimelineRepository
{
    Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(CancellationToken ct = default);
    Task AddAsync(TimelineEvent timelineEvent, CancellationToken ct = default);
}
