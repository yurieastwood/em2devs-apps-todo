using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresStreakSnapshotRepository : IStreakSnapshotRepository
{
    private readonly TodoDbContext _dbContext;

    public PostgresStreakSnapshotRepository(TodoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(StreakSnapshot snapshot, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _dbContext.StreakSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<StreakSnapshot?> GetByDateAsync(DateOnly snapshotDate, CancellationToken ct = default)
    {
        return await _dbContext.StreakSnapshots
            .FirstOrDefaultAsync(s => s.SnapshotDate == snapshotDate, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<StreakSnapshot>> GetRangeAsync(DateOnly fromInclusive, DateOnly toInclusive, CancellationToken ct = default)
    {
        return await _dbContext.StreakSnapshots
            .Where(s => s.SnapshotDate >= fromInclusive && s.SnapshotDate <= toInclusive)
            .OrderBy(s => s.SnapshotDate)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
