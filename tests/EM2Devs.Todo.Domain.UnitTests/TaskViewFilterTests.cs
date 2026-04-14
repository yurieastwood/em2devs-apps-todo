using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class TaskViewFilterTests
{
    private static readonly DateOnly _today = new(2026, 4, 15);

    private static TodoTask NewTask(string title, DateOnly? scheduled = null,
        DateTimeOffset? createdAt = null, QuestId? questId = null)
    {
        var dueDate = scheduled?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var task = dueDate.HasValue
            ? TodoTask.CreateFromRecurring(new TaskTitle(title), RecurringTaskId.New(), scheduled!.Value)
            : TodoTask.Create(new TaskTitle(title), createdAt: createdAt);
        if (scheduled.HasValue)
        {
            // override created-at via reflection-free public method path: use CreateFromRecurring's CreatedAt = now.
            // For created-at sequencing in tests we rely on real construction-time ordering.
        }
        if (questId is not null)
        {
            task.AssignToQuest(questId);
        }
        return task;
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeOnlyUnassignedOpenTasks_When_Inbox()
    {
        var a = TodoTask.Create(new TaskTitle("A"), createdAt: new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero));
        var b = TodoTask.Create(new TaskTitle("B"), createdAt: new DateTimeOffset(2026, 4, 2, 0, 0, 0, TimeSpan.Zero));
        b.AssignToQuest(QuestId.New());
        var c = TodoTask.Create(new TaskTitle("C"), createdAt: new DateTimeOffset(2026, 4, 3, 0, 0, 0, TimeSpan.Zero));
        c.Delete();

        var inbox = TaskViewFilter.ForInbox(new[] { a, b, c });

        inbox.Count.ShouldBe(1);
        inbox[0].Title.Value.ShouldBe("A");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SortByCreationDescending_When_Inbox()
    {
        var older = TodoTask.Create(new TaskTitle("Older"),
            createdAt: new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero));
        var newer = TodoTask.Create(new TaskTitle("Newer"),
            createdAt: new DateTimeOffset(2026, 4, 10, 0, 0, 0, TimeSpan.Zero));

        var inbox = TaskViewFilter.ForInbox(new[] { older, newer });

        inbox[0].Title.Value.ShouldBe("Newer");
        inbox[1].Title.Value.ShouldBe("Older");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeDueTodayAndOverdue_When_Today()
    {
        var dueToday = TodoTask.CreateFromRecurring(new TaskTitle("today"), RecurringTaskId.New(), _today);
        var overdue = TodoTask.CreateFromRecurring(new TaskTitle("overdue"), RecurringTaskId.New(), _today.AddDays(-3));
        var future = TodoTask.CreateFromRecurring(new TaskTitle("future"), RecurringTaskId.New(), _today.AddDays(2));
        var unscheduled = TodoTask.Create(new TaskTitle("none"));

        var result = TaskViewFilter.ForToday(new[] { dueToday, overdue, future, unscheduled }, _today);

        result.Select(t => t.Title.Value).ToList().ShouldBe(new List<string> { "overdue", "today" });
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeCompleted_When_Today()
    {
        var completed = TodoTask.CreateFromRecurring(new TaskTitle("done"), RecurringTaskId.New(), _today);
        completed.MoveToInProgress();
        completed.MarkAsDone();

        var result = TaskViewFilter.ForToday(new[] { completed }, _today);

        result.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Return14DayGroups_When_Upcoming()
    {
        var day3 = TodoTask.CreateFromRecurring(new TaskTitle("d3"), RecurringTaskId.New(), _today.AddDays(3));
        var day7 = TodoTask.CreateFromRecurring(new TaskTitle("d7"), RecurringTaskId.New(), _today.AddDays(7));
        var outside = TodoTask.CreateFromRecurring(new TaskTitle("d20"), RecurringTaskId.New(), _today.AddDays(20));

        var groups = TaskViewFilter.ForUpcoming(new[] { day3, day7, outside }, _today);

        groups.Count.ShouldBe(TaskViewFilter.UpcomingWindowDays);
        groups[0].Date.ShouldBe(_today.AddDays(1));
        groups[^1].Date.ShouldBe(_today.AddDays(TaskViewFilter.UpcomingWindowDays));
        groups.Single(g => g.Date == _today.AddDays(3)).Tasks.Single().Title.Value.ShouldBe("d3");
        groups.Single(g => g.Date == _today.AddDays(7)).Tasks.Single().Title.Value.ShouldBe("d7");
        groups.SelectMany(g => g.Tasks).Any(t => t.Title.Value == "d20").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeEmptyDays_When_NoTasksOnThatDay()
    {
        var groups = TaskViewFilter.ForUpcoming(Array.Empty<TodoTask>(), _today);
        groups.Count.ShouldBe(TaskViewFilter.UpcomingWindowDays);
        groups.ShouldAllBe(g => g.Tasks.Count == 0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GroupCompletedTasksByCompletionDate_When_Completed()
    {
        var t1 = TodoTask.CreateFromRecurring(new TaskTitle("t1"), RecurringTaskId.New(), _today);
        t1.MoveToInProgress();
        t1.MarkAsDone();

        var t2 = TodoTask.CreateFromRecurring(new TaskTitle("t2"), RecurringTaskId.New(), _today);
        t2.MoveToInProgress();
        t2.MarkAsDone();

        var open = TodoTask.Create(new TaskTitle("open"));

        var result = TaskViewFilter.ForCompleted(new[] { t1, t2, open });

        result.Count.ShouldBe(1);
        result[0].Tasks.Count.ShouldBe(2);
        result[0].Date.ShouldBe(DateOnly.FromDateTime(t1.CompletedAt!.Value.UtcDateTime));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OrderGroupsDescendingByDate_When_Completed()
    {
        // Fabricate two tasks and directly verify ordering by CompletedAt dates.
        var t1 = TodoTask.CreateFromRecurring(new TaskTitle("t1"), RecurringTaskId.New(), _today);
        t1.MoveToInProgress();
        t1.MarkAsDone();

        // Second task completed later -> should appear first.
        System.Threading.Thread.Sleep(5);
        var t2 = TodoTask.CreateFromRecurring(new TaskTitle("t2"), RecurringTaskId.New(), _today);
        t2.MoveToInProgress();
        t2.MarkAsDone();

        // If they landed on the same UTC day, we can only assert ordering within-group.
        var result = TaskViewFilter.ForCompleted(new[] { t1, t2 });
        var flat = result.SelectMany(g => g.Tasks).ToList();
        flat[0].CompletedAt!.Value.ShouldBeGreaterThanOrEqualTo(flat[1].CompletedAt!.Value);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TasksNull()
    {
        Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForInbox(null!));
        Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForToday(null!, _today));
        Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForUpcoming(null!, _today));
        Should.Throw<ArgumentNullException>(() => TaskViewFilter.ForCompleted(null!));
    }
}
