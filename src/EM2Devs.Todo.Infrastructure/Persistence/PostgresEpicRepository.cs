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
        Epic? epic = await _dbContext.Epics
            .FirstOrDefaultAsync(e => e.Id == id, ct)
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
        // TODO(perf): two-level N+1 — 1 query for all epics, plus 1 per epic for quests, plus 1 per quest for tasks.
        // For E epics with Q quests each, total is 1 + E + (E × Q) queries.
        // Acceptable at current scale; see design spec docs/superpowers/specs/2026-05-03-postgres-persistence-design.md.
        List<Epic> epics = await _dbContext.Epics.ToListAsync(ct).ConfigureAwait(false);
        foreach (Epic e in epics)
        {
            await HydrateQuestsAsync(e, ct).ConfigureAwait(false);
        }
        return epics;
    }

    public async Task SaveAsync(Epic epic, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(epic);
        if (_dbContext.Entry(epic).State == EntityState.Detached)
        {
            _dbContext.Epics.Add(epic);
        }
        // Note: child Quests are NOT persisted here — QuestRepository owns them.
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<bool> DeleteAsync(EpicId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Epic? epic = await _dbContext.Epics
            .FirstOrDefaultAsync(e => e.Id == id, ct)
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
        // Quests have no user_id column (Quest/Epic multi-tenancy is a known gap); scoped only by epic.
        List<Quest> quests = await _dbContext.Quests
            .Where(q => q.EpicId == epicId)
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
