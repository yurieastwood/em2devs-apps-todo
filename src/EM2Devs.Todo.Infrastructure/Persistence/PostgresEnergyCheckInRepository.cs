using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EM2Devs.Todo.Infrastructure.Persistence;

// TODO(ADR-029): candidate for Dapper migration — read-model trio per ADR-009.
public sealed class PostgresEnergyCheckInRepository : IEnergyCheckInRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresEnergyCheckInRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<EnergyCheckIn>> GetRecentAsync(int days = 60, CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-days);
        return await _dbContext.EnergyCheckIns
            .Where(c => EF.Property<Guid>(c, "UserId") == userId && c.RecordedAt >= cutoff)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<EnergyCheckIn?> GetTodayAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateTimeOffset startOfDay = new(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        DateTimeOffset startOfNextDay = new(today.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        return await _dbContext.EnergyCheckIns
            .Where(c => EF.Property<Guid>(c, "UserId") == userId
                     && c.RecordedAt >= startOfDay
                     && c.RecordedAt < startOfNextDay)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(EnergyCheckIn checkIn, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);
        EntityEntry<EnergyCheckIn> entry = _dbContext.EnergyCheckIns.Add(checkIn);
        entry.Property("UserId").CurrentValue = _currentUser.UserId;
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task UpdateAsync(EnergyCheckIn checkIn, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
