using EM2Devs.Todo.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Issues per-table DELETE statements scoped to the current user across every
/// user-scoped DbSet. StreakSnapshot is intentionally skipped — its entity has no
/// UserId column (single-user demo mode); see <see cref="IUserDataPurger"/>.
/// </summary>
public sealed class PostgresUserDataPurger : IUserDataPurger
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresUserDataPurger(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task PurgeAllForCurrentUserAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;

        await _dbContext.Tasks
            .Where(t => t.UserId == userId).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.RecurringTasks
            .Where(r => r.UserId == userId).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.Notifications
            .Where(n => n.UserId == userId).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.PlayerProfiles
            .Where(p => p.UserId == userId).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.WeeklyReflections
            .Where(w => w.UserId == userId).ExecuteDeleteAsync(ct).ConfigureAwait(false);

        // Shadow-property UserId for InsightCard, EnergyCheckIn, TimelineEvent, Quest, Epic.
        await _dbContext.InsightCards
            .Where(c => EF.Property<Guid>(c, "UserId") == userId)
            .ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.EnergyCheckIns
            .Where(c => EF.Property<Guid>(c, "UserId") == userId)
            .ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.TimelineEvents
            .Where(e => EF.Property<Guid>(e, "UserId") == userId)
            .ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.Quests
            .Where(q => EF.Property<Guid>(q, "UserId") == userId)
            .ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await _dbContext.Epics
            .Where(e => EF.Property<Guid>(e, "UserId") == userId)
            .ExecuteDeleteAsync(ct).ConfigureAwait(false);
    }
}
