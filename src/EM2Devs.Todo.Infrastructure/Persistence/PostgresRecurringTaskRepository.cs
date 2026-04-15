using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresRecurringTaskRepository : IRecurringTaskRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresRecurringTaskRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<RecurringTask?> GetByIdAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;
        return await _dbContext.RecurringTasks
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<RecurringTask>> GetAllAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        return await _dbContext.RecurringTasks
            .Where(r => r.UserId == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task SaveAsync(RecurringTask recurringTask, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(recurringTask);
        if (recurringTask.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "RecurringTask UserId does not match the current user. Cross-user writes are forbidden.");
        }

        if (_dbContext.Entry(recurringTask).State == EntityState.Detached)
        {
            _dbContext.RecurringTasks.Add(recurringTask);
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(RecurringTaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;

        RecurringTask? recurring = await _dbContext.RecurringTasks
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct)
            .ConfigureAwait(false);
        if (recurring is null)
        {
            return false;
        }

        _dbContext.RecurringTasks.Remove(recurring);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<RecurringTask>> GetAllForGenerationAsync(CancellationToken ct = default)
    {
        return await _dbContext.RecurringTasks
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
