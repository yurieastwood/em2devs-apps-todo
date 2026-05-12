namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Permanently deletes every record belonging to the current authenticated user
/// across the user-scoped repositories. The <c>User</c> account row itself is preserved
/// (the scenario calls this "active but empty").
///
/// StreakSnapshot is intentionally skipped: that table has no UserId column
/// (single-user demo mode comment in the entity); fixing it is a separate slice.
/// </summary>
public interface IUserDataPurger
{
    Task PurgeAllForCurrentUserAsync(CancellationToken ct = default);
}
