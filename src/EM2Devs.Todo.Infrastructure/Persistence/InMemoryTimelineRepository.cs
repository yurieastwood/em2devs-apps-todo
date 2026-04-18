using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class InMemoryTimelineStore
{
    private readonly Dictionary<Guid, List<TimelineEvent>> _events = new();
    private readonly object _lock = new();

    public IReadOnlyList<TimelineEvent> GetForUser(Guid userId)
    {
        lock (_lock)
        {
            return _events.TryGetValue(userId, out List<TimelineEvent>? list)
                ? list.AsReadOnly()
                : [];
        }
    }

    public void Add(Guid userId, TimelineEvent timelineEvent)
    {
        lock (_lock)
        {
            if (!_events.TryGetValue(userId, out List<TimelineEvent>? list))
            {
                list = [];
                _events[userId] = list;
            }

            list.Add(timelineEvent);
        }
    }
}

public sealed class InMemoryTimelineRepository : ITimelineRepository
{
    private readonly InMemoryTimelineStore _store;
    private readonly ICurrentUser _currentUser;

    public InMemoryTimelineRepository(InMemoryTimelineStore store, ICurrentUser currentUser)
    {
        _store = store;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_store.GetForUser(_currentUser.UserId));
    }

    public Task AddAsync(TimelineEvent timelineEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(timelineEvent);
        _store.Add(_currentUser.UserId, timelineEvent);
        return Task.CompletedTask;
    }
}
