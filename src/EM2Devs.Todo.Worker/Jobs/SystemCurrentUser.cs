using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Worker.Jobs;

/// <summary>
/// Slice 1 fallback <see cref="ICurrentUser"/> used by background jobs that run outside
/// any HTTP request scope. Returns the seed demo user so cross-user filters in
/// <c>PostgresTaskRepository</c> still work inside the worker. Removed in Slice 2 when
/// the job gains per-user awareness (e.g., iterating owners of RecurringTasks).
/// </summary>
internal sealed class SystemCurrentUser : ICurrentUser
{
    private static readonly Guid _demoUserId = new("00000000-0000-0000-0000-000000000001");

    public Guid UserId => _demoUserId;
    public string DisplayName => "System";
    public bool IsAuthenticated => true;
}
