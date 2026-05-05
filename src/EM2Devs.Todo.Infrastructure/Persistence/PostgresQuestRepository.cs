using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresQuestRepository : IQuestRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresQuestRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Quest?> GetByIdAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Quest? quest = await _dbContext.Quests
            .FirstOrDefaultAsync(q => q.Id == id, ct)
            .ConfigureAwait(false);
        if (quest is null)
        {
            return null;
        }

        await HydrateTasksAsync(quest, ct).ConfigureAwait(false);
        return quest;
    }

    public async Task<IReadOnlyList<Quest>> GetAllAsync(CancellationToken ct = default)
    {
        // TODO(perf): batch task hydration — currently N+1 (one query per Quest).
        // Acceptable at current scale; see design spec docs/superpowers/specs/2026-05-03-postgres-persistence-design.md.
        List<Quest> quests = await _dbContext.Quests.ToListAsync(ct).ConfigureAwait(false);
        foreach (Quest q in quests)
        {
            await HydrateTasksAsync(q, ct).ConfigureAwait(false);
        }
        return quests;
    }

    public async Task<IReadOnlyList<Quest>> GetByTaskIdAsync(TaskId taskId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        Guid userId = _currentUser.UserId;
        List<QuestId> questIds = await _dbContext.Tasks
            .Where(t => t.UserId == userId && t.Id == taskId && t.AssignedQuestId != null)
            .Select(t => t.AssignedQuestId!)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (questIds.Count == 0)
        {
            return [];
        }

        List<Quest> quests = await _dbContext.Quests
            .Where(q => questIds.Contains(q.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);
        foreach (Quest q in quests)
        {
            await HydrateTasksAsync(q, ct).ConfigureAwait(false);
        }
        return quests;
    }

    public async Task SaveAsync(Quest quest, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(quest);
        if (_dbContext.Entry(quest).State == EntityState.Detached)
        {
            _dbContext.Quests.Add(quest);
        }
        // Note: child tasks are NOT persisted here — TaskRepository owns them.
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Quest? quest = await _dbContext.Quests
            .FirstOrDefaultAsync(q => q.Id == id, ct)
            .ConfigureAwait(false);
        if (quest is null)
        {
            return false;
        }

        _dbContext.Quests.Remove(quest);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    private async Task HydrateTasksAsync(Quest quest, CancellationToken ct)
    {
        Guid userId = _currentUser.UserId;
        QuestId questId = quest.Id;
        List<TodoTask> tasks = await _dbContext.Tasks
            .Where(t => t.UserId == userId && t.AssignedQuestId == questId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        foreach (TodoTask task in tasks)
        {
            quest.AddTask(task);
        }
    }
}
