using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

public sealed class PostgresEpicRepository : IEpicRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresEpicRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Epic?> GetByIdAsync(EpicId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;
        Epic? epic = await _dbContext.Epics
            .FirstOrDefaultAsync(e => e.Id == id && EF.Property<Guid>(e, "UserId") == userId, ct)
            .ConfigureAwait(false);
        if (epic is null)
        {
            return null;
        }

        await HydrateQuestsAsync(epic, ct).ConfigureAwait(false);
        return epic;
    }

    public async Task<IReadOnlyList<Epic>> GetAllAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        List<Epic> epics = await _dbContext.Epics
            .Where(e => EF.Property<Guid>(e, "UserId") == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);
        if (epics.Count == 0)
        {
            return epics;
        }

        // Batched hydration: 3 queries total regardless of fan-out.
        // (was 1 + E + (E × Q) before the rewrite).
        List<EpicId> epicIds = epics.ConvertAll(e => e.Id);
        List<Quest> quests = await _dbContext.Quests
            .Where(q => q.EpicId != null
                && epicIds.Contains(q.EpicId)
                && EF.Property<Guid>(q, "UserId") == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        ILookup<QuestId, TodoTask> tasksByQuest;
        if (quests.Count == 0)
        {
            tasksByQuest = Array.Empty<TodoTask>().ToLookup(t => t.AssignedQuestId!);
        }
        else
        {
            List<QuestId> questIds = quests.ConvertAll(q => q.Id);
            List<TodoTask> tasks = await _dbContext.Tasks
                .Where(t => t.UserId == userId
                    && t.AssignedQuestId != null
                    && questIds.Contains(t.AssignedQuestId))
                .ToListAsync(ct)
                .ConfigureAwait(false);
            tasksByQuest = tasks.ToLookup(t => t.AssignedQuestId!);
        }

        ILookup<EpicId, Quest> questsByEpic = quests.ToLookup(q => q.EpicId!);
        foreach (Epic epic in epics)
        {
            foreach (Quest quest in questsByEpic[epic.Id])
            {
                foreach (TodoTask task in tasksByQuest[quest.Id])
                {
                    quest.AddTask(task);
                }
                epic.AddQuest(quest);
            }
        }
        return epics;
    }

    public async Task SaveAsync(Epic epic, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(epic);
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Epic> entry = _dbContext.Entry(epic);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Epics.Add(epic);
            entry = _dbContext.Entry(epic);
        }
        entry.Property("UserId").CurrentValue = _currentUser.UserId;
        // Note: child Quests are NOT persisted here — QuestRepository owns them.
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(EpicId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;
        Epic? epic = await _dbContext.Epics
            .FirstOrDefaultAsync(e => e.Id == id && EF.Property<Guid>(e, "UserId") == userId, ct)
            .ConfigureAwait(false);
        if (epic is null)
        {
            return false;
        }

        _dbContext.Epics.Remove(epic);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    private async Task HydrateQuestsAsync(Epic epic, CancellationToken ct)
    {
        EpicId epicId = epic.Id;
        Guid userId = _currentUser.UserId;
        List<Quest> quests = await _dbContext.Quests
            .Where(q => q.EpicId == epicId && EF.Property<Guid>(q, "UserId") == userId)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        foreach (Quest quest in quests)
        {
            QuestId questId = quest.Id;
            List<TodoTask> tasks = await _dbContext.Tasks
                .Where(t => t.UserId == userId && t.AssignedQuestId == questId)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            foreach (TodoTask task in tasks)
            {
                quest.AddTask(task);
            }
            epic.AddQuest(quest);
        }
    }
}
