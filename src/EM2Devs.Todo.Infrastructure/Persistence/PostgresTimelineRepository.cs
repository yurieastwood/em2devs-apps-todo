using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EM2Devs.Todo.Infrastructure.Persistence;

// TODO(ADR-029): candidate for Dapper migration — read-model trio per ADR-009.
public sealed class PostgresTimelineRepository : ITimelineRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresTimelineRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        return await _dbContext.TimelineEvents
            .Where(e => EF.Property<Guid>(e, "UserId") == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(TimelineEvent timelineEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(timelineEvent);
        EntityEntry<TimelineEvent> entry = _dbContext.TimelineEvents.Add(timelineEvent);
        entry.Property("UserId").CurrentValue = _currentUser.UserId;
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
