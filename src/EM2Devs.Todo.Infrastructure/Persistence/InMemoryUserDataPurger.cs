using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// Clears every per-user entry from the in-memory stores. StreakSnapshot is the only
/// remaining unscoped store (single-user demo mode in the entity); see
/// <see cref="IUserDataPurger"/> for the rationale.
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
    private readonly InMemoryQuestStore _quests;
    private readonly InMemoryEpicStore _epics;
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
        InMemoryQuestStore quests,
        InMemoryEpicStore epics,
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
        _quests = quests;
        _epics = epics;
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

        foreach ((Guid UserId, DateOnly WeekOf) key in _reflections.Reflections.Keys.Where(k => k.UserId == userId).ToList())
        {
            _reflections.Reflections.TryRemove(key, out _);
        }

        _insights.RemoveAllForUser(userId);
        _energy.RemoveAllForUser(userId);
        _timeline.RemoveAllForUser(userId);

        foreach ((Guid UserId, Guid QuestId) key in _quests.Quests.Keys.Where(k => k.UserId == userId).ToList())
        {
            _quests.Quests.TryRemove(key, out _);
        }
        foreach ((Guid UserId, Guid EpicId) key in _epics.Epics.Keys.Where(k => k.UserId == userId).ToList())
        {
            _epics.Epics.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }
}
