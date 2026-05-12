using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        Guid userId = _currentUser.UserId;
        Quest? quest = await _dbContext.Quests
            .FirstOrDefaultAsync(q => q.Id == id && EF.Property<Guid>(q, "UserId") == userId, ct)
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
        Guid userId = _currentUser.UserId;
        List<Quest> quests = await _dbContext.Quests
            .Where(q => EF.Property<Guid>(q, "UserId") == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (quests.Count == 0)
        {
            return quests;
        }

        // Batched task hydration: a single query keyed by IN (questIds) replaces the
        // prior per-quest fetch (1 + N queries → 2 total).
        List<QuestId> questIds = quests.ConvertAll(q => q.Id);
        List<TodoTask> tasks = await _dbContext.Tasks
            .Where(t => t.UserId == userId
                && t.AssignedQuestId != null
                && questIds.Contains(t.AssignedQuestId))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        ILookup<QuestId, TodoTask> tasksByQuest = tasks.ToLookup(t => t.AssignedQuestId!);
        foreach (Quest q in quests)
        {
            foreach (TodoTask t in tasksByQuest[q.Id])
            {
                q.AddTask(t);
            }
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
            .Where(q => questIds.Contains(q.Id) && EF.Property<Guid>(q, "UserId") == userId)
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
        EntityEntry<Quest> entry = _dbContext.Entry(quest);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Quests.Add(quest);
            entry = _dbContext.Entry(quest);
        }
        entry.Property("UserId").CurrentValue = _currentUser.UserId;
        // Note: child tasks are NOT persisted here — TaskRepository owns them.
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(QuestId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;
        Quest? quest = await _dbContext.Quests
            .FirstOrDefaultAsync(q => q.Id == id && EF.Property<Guid>(q, "UserId") == userId, ct)
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
