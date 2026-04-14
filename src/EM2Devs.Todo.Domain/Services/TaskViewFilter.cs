using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Services;

/// <summary>
/// Pure domain service that filters tasks for a given named view.
/// Inbox: tasks not assigned to any quest, sorted by creation date descending.
/// Today: tasks due today plus any overdue open tasks.
/// Upcoming: tasks due within the next 14 days, grouped by due date.
/// Completed: completed tasks, grouped by completion date (most recent first).
/// </summary>
public static class TaskViewFilter
{
    /// <summary>Number of days included in the Upcoming view (inclusive of today+1..today+14).</summary>
    public const int UpcomingWindowDays = 14;

    public static IReadOnlyList<TodoTask> ForInbox(IReadOnlyList<TodoTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        return tasks
            .Where(t => t.AssignedQuestId is null
                && t.Status != TaskStatus.Done
                && t.Status != TaskStatus.Deleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    public static IReadOnlyList<TodoTask> ForToday(IReadOnlyList<TodoTask> tasks, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        return tasks
            .Where(t => t.Status != TaskStatus.Done
                && t.Status != TaskStatus.Deleted
                && t.ScheduledDate.HasValue
                && t.ScheduledDate.Value <= today)
            .OrderBy(t => t.ScheduledDate!.Value)
            .ToList();
    }

    /// <summary>
    /// Returns the upcoming view as a list of day-groups covering today+1 through today+N,
    /// where N is <see cref="UpcomingWindowDays"/>. Days with no tasks are still returned
    /// with an empty task list so the UI can render them.
    /// </summary>
    public static IReadOnlyList<TaskDayGroup> ForUpcoming(IReadOnlyList<TodoTask> tasks, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        var groups = new List<TaskDayGroup>(UpcomingWindowDays);
        for (int offset = 1; offset <= UpcomingWindowDays; offset++)
        {
            DateOnly day = today.AddDays(offset);
            var dayTasks = tasks
                .Where(t => t.Status != TaskStatus.Done
                    && t.Status != TaskStatus.Deleted
                    && t.ScheduledDate == day)
                .OrderBy(t => t.CreatedAt)
                .ToList();
            groups.Add(new TaskDayGroup(day, dayTasks));
        }
        return groups;
    }

    /// <summary>
    /// Returns completed tasks grouped by the date they were completed (UTC),
    /// with the most recent group first.
    /// </summary>
    public static IReadOnlyList<TaskDayGroup> ForCompleted(IReadOnlyList<TodoTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        return tasks
            .Where(t => t.Status == TaskStatus.Done && t.CompletedAt.HasValue)
            .GroupBy(t => DateOnly.FromDateTime(t.CompletedAt!.Value.UtcDateTime))
            .OrderByDescending(g => g.Key)
            .Select(g => new TaskDayGroup(
                g.Key,
                g.OrderByDescending(t => t.CompletedAt!.Value).ToList()))
            .ToList();
    }
}

/// <summary>
/// A group of tasks associated with a specific calendar day.
/// </summary>
public sealed record TaskDayGroup(DateOnly Date, IReadOnlyList<TodoTask> Tasks);
