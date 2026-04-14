using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Queries;

public sealed class ListTasksQueryViewTests
{
    private static readonly DateTimeOffset _fixedNow = new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);
    private static DateOnly Today => DateOnly.FromDateTime(_fixedNow.UtcDateTime);

    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly TimeProvider _timeProvider = new FixedTimeProvider(_fixedNow);
    private readonly ListTasksQueryHandler _handler;

    public ListTasksQueryViewTests()
    {
        _handler = new ListTasksQueryHandler(_repository, _timeProvider);
    }

    private sealed class FixedTimeProvider(DateTimeOffset fixedNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedNow;
    }

    private void SeedTasks(params TodoTask[] tasks) =>
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tasks);

    private static TodoTask CreateScheduled(string title, DateOnly scheduledDate)
    {
        return TodoTask.CreateFromRecurring(
            new TaskTitle(title),
            RecurringTaskId.New(),
            scheduledDate);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnInboxTasks_When_ViewIsInbox()
    {
        // Inbox = open tasks with no assigned quest. Completed/Deleted tasks are excluded.
        TodoTask open = TodoTask.Create(new TaskTitle("Open inbox item"));
        TodoTask completed = TodoTask.Create(new TaskTitle("Completed item"));
        completed.MoveToInProgress();
        completed.MarkAsDone();
        SeedTasks(open, completed);

        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery(null, "inbox"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        IReadOnlyList<TodoTask> tasks = result.Match(t => t, _ => []);
        tasks.ShouldHaveSingleItem().Title.Value.ShouldBe("Open inbox item");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnTodaysTasks_When_ViewIsToday()
    {
        TodoTask scheduledToday = CreateScheduled("Due today", Today);
        TodoTask scheduledTomorrow = CreateScheduled("Due tomorrow", Today.AddDays(1));
        TodoTask withoutSchedule = TodoTask.Create(new TaskTitle("No date"));
        SeedTasks(scheduledToday, scheduledTomorrow, withoutSchedule);

        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery(null, "today"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        IReadOnlyList<TodoTask> tasks = result.Match(t => t, _ => []);
        tasks.ShouldHaveSingleItem().Title.Value.ShouldBe("Due today");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnUpcomingTasks_When_ViewIsUpcoming()
    {
        TodoTask scheduledToday = CreateScheduled("Due today", Today);
        TodoTask scheduledTomorrow = CreateScheduled("Due tomorrow", Today.AddDays(1));
        TodoTask scheduledInTwoWeeks = CreateScheduled("Too far out", Today.AddDays(30));
        SeedTasks(scheduledToday, scheduledTomorrow, scheduledInTwoWeeks);

        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery(null, "upcoming"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        IReadOnlyList<TodoTask> tasks = result.Match(t => t, _ => []);
        tasks.ShouldHaveSingleItem().Title.Value.ShouldBe("Due tomorrow");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnCompletedTasks_When_ViewIsCompleted()
    {
        TodoTask active = CreateScheduled("Open", Today);
        TodoTask completed = TodoTask.Create(new TaskTitle("Completed"));
        completed.MoveToInProgress();
        completed.MarkAsDone();
        SeedTasks(active, completed);

        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery(null, "completed"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        IReadOnlyList<TodoTask> tasks = result.Match(t => t, _ => []);
        tasks.ShouldHaveSingleItem().Title.Value.ShouldBe("Completed");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_IgnoreCase_When_ViewNameIsMixedCase()
    {
        TodoTask withoutSchedule = TodoTask.Create(new TaskTitle("Inbox item"));
        SeedTasks(withoutSchedule);

        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery(null, "Inbox"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Count, _ => -1).ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_ViewIsInvalid()
    {
        SeedTasks();

        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery(null, "nonsense"), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => string.Empty, e => e.Message).ShouldContain("Invalid view");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_ViewAndStatusBothProvided()
    {
        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery("Todo", "inbox"), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        ResultError error = result.Match(_ => (ResultError)new ValidationError(""), e => e);
        error.ShouldBeOfType<ValidationError>();
        error.Message.ShouldContain("mutually exclusive");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ApplyStatusFilter_When_OnlyStatusProvided()
    {
        TodoTask todo = TodoTask.Create(new TaskTitle("Todo item"));
        TodoTask done = TodoTask.Create(new TaskTitle("Done item"));
        done.MoveToInProgress();
        done.MarkAsDone();
        SeedTasks(todo, done);

        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(new ListTasksQuery("Done"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        IReadOnlyList<TodoTask> tasks = result.Match(t => t, _ => []);
        tasks.ShouldHaveSingleItem().Title.Value.ShouldBe("Done item");
    }
}
