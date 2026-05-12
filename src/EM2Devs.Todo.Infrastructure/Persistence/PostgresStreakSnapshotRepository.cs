using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresStreakSnapshotRepository : IStreakSnapshotRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresStreakSnapshotRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task SaveAsync(StreakSnapshot snapshot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _dbContext.StreakSnapshots.Add(snapshot);
        try
        {
            await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            // Unique constraint on (user_id, snapshot_date) — another process already wrote
            // this user's snapshot for the day. Treat as idempotent success: clear tracker.
            _dbContext.ChangeTracker.Clear();
        }
    }

    public async Task<StreakSnapshot?> GetByDateAsync(DateOnly snapshotDate, CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        return await _dbContext.StreakSnapshots
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SnapshotDate == snapshotDate, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<StreakSnapshot>> GetRangeAsync(DateOnly fromInclusive, DateOnly toInclusive, CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        return await _dbContext.StreakSnapshots
            .Where(s => s.UserId == userId && s.SnapshotDate >= fromInclusive && s.SnapshotDate <= toInclusive)
            .OrderBy(s => s.SnapshotDate)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
