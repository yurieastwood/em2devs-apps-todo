using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresRecurringTaskRepository : IRecurringTaskRepository
{
    private readonly TodoDbContext _dbContext;

    public PostgresRecurringTaskRepository(TodoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RecurringTask?> GetByIdAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _dbContext.RecurringTasks.FindAsync([id], ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RecurringTask>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.RecurringTasks.ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task SaveAsync(RecurringTask recurringTask, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(recurringTask);

        bool exists = await _dbContext.RecurringTasks
            .AnyAsync(r => r.Id == recurringTask.Id, ct)
            .ConfigureAwait(false);

        if (!exists)
        {
            _dbContext.RecurringTasks.Add(recurringTask);
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        RecurringTask? recurring = await _dbContext.RecurringTasks.FindAsync([id], ct).ConfigureAwait(false);
        if (recurring is null)
        {
            return false;
        }

        _dbContext.RecurringTasks.Remove(recurring);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }
}
