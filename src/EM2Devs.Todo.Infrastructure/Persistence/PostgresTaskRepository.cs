using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresTaskRepository : ITaskRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresTaskRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<TodoTask?> GetByIdAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;
        return await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        return await _dbContext.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task SaveAsync(TodoTask task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (task.UserId != _currentUser.UserId)
        {
            throw new InvalidOperationException(
                "Task UserId does not match the current user. Cross-user writes are forbidden.");
        }

        if (_dbContext.Entry(task).State == EntityState.Detached)
        {
            _dbContext.Tasks.Add(task);
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        Guid userId = _currentUser.UserId;
        TodoTask? task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct)
            .ConfigureAwait(false);
        if (task is null)
        {
            return false;
        }

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<TodoTask>> GetByRecurringTaskIdAsync(RecurringTaskId sourceId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        Guid userId = _currentUser.UserId;
        return await _dbContext.Tasks
            .Where(t => t.UserId == userId && t.SourceRecurringTaskId == sourceId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<DateOnly?> GetMaxScheduledDateAsync(RecurringTaskId sourceId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        Guid userId = _currentUser.UserId;
        return await _dbContext.Tasks
            .Where(t => t.UserId == userId && t.SourceRecurringTaskId == sourceId && t.ScheduledDate != null)
            .MaxAsync(t => t.ScheduledDate, ct)
            .ConfigureAwait(false);
    }
}
