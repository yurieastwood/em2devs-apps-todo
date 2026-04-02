using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresTaskRepository : ITaskRepository
{
    private readonly TodoDbContext _dbContext;

    public PostgresTaskRepository(TodoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TodoTask?> GetByIdAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await _dbContext.Tasks.FindAsync([id], ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TodoTask>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbContext.Tasks.ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task SaveAsync(TodoTask task, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(task);

        bool exists = await _dbContext.Tasks
            .AnyAsync(t => t.Id == task.Id, ct)
            .ConfigureAwait(false);

        if (!exists)
        {
            _dbContext.Tasks.Add(task);
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(TaskId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        TodoTask? task = await _dbContext.Tasks.FindAsync([id], ct).ConfigureAwait(false);
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
        return await _dbContext.Tasks
            .Where(t => t.SourceRecurringTaskId == sourceId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
