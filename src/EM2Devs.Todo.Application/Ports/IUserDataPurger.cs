namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Permanently deletes every record belonging to the current authenticated user
/// across the user-scoped repositories. The <c>User</c> account row itself is preserved
/// (the scenario calls this "active but empty").
///
/// Quest, Epic, and StreakSnapshot are intentionally skipped: those tables are not
/// user-scoped at the repository layer today (pre-existing data-model gap). Tasks the
/// user owned that were assigned to a quest are deleted, leaving any global quest/epic
/// records orphaned.
/// </summary>
public interface IUserDataPurger
{
    Task PurgeAllForCurrentUserAsync(CancellationToken ct = default);
}
