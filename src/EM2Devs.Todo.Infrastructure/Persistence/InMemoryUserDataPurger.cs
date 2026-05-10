using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Clears every per-user entry from the in-memory stores. Quest and Epic stores are
/// skipped because they are global-keyed today (no per-user partition); StreakSnapshot
/// has the same gap. See <see cref="IUserDataPurger"/> for the rationale.
/// </summary>
public sealed class InMemoryUserDataPurger : IUserDataPurger
{
    private readonly InMemoryTaskStore _tasks;
    private readonly InMemoryRecurringTaskStore _recurringTasks;
    private readonly InMemoryNotificationStore _notifications;
    private readonly InMemoryPlayerProfileStore _profiles;
    private readonly InMemoryWeeklyReflectionStore _reflections;
    private readonly InMemoryInsightCardStore _insights;
    private readonly InMemoryEnergyCheckInStore _energy;
    private readonly InMemoryTimelineStore _timeline;
    private readonly ICurrentUser _currentUser;

    public InMemoryUserDataPurger(
        InMemoryTaskStore tasks,
        InMemoryRecurringTaskStore recurringTasks,
        InMemoryNotificationStore notifications,
        InMemoryPlayerProfileStore profiles,
        InMemoryWeeklyReflectionStore reflections,
        InMemoryInsightCardStore insights,
        InMemoryEnergyCheckInStore energy,
        InMemoryTimelineStore timeline,
        ICurrentUser currentUser)
    {
        _tasks = tasks;
        _recurringTasks = recurringTasks;
        _notifications = notifications;
        _profiles = profiles;
        _reflections = reflections;
        _insights = insights;
        _energy = energy;
        _timeline = timeline;
        _currentUser = currentUser;
    }

    public Task PurgeAllForCurrentUserAsync(CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;

        foreach (Guid id in _tasks.Tasks.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Key).ToList())
        {
            _tasks.Tasks.TryRemove(id, out _);
        }
        foreach (Guid id in _recurringTasks.RecurringTasks.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Key).ToList())
        {
            _recurringTasks.RecurringTasks.TryRemove(id, out _);
        }
        foreach (Guid id in _notifications.Notifications.Where(kvp => kvp.Value.UserId == userId).Select(kvp => kvp.Key).ToList())
        {
            _notifications.Notifications.TryRemove(id, out _);
        }

        _profiles.Profiles.TryRemove(userId, out _);

        foreach (var key in _reflections.Reflections.Keys.Where(k => k.UserId == userId).ToList())
        {
            _reflections.Reflections.TryRemove(key, out _);
        }

        _insights.RemoveAllForUser(userId);
        _energy.RemoveAllForUser(userId);
        _timeline.RemoveAllForUser(userId);

        return Task.CompletedTask;
    }
}
