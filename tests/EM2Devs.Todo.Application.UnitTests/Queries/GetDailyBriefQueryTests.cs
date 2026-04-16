using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Queries;

[Trait("Category", "Application")]
public sealed class GetDailyBriefQueryTests
{
    private static readonly DateTimeOffset _fixedNow = new(2026, 4, 12, 9, 0, 0, TimeSpan.Zero);
    private static DateOnly Today => DateOnly.FromDateTime(_fixedNow.UtcDateTime);

    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly IPlayerProfileRepository _profileRepository = Substitute.For<IPlayerProfileRepository>();
    private readonly ICurrentUser _currentUser = new FakeCurrentUser(TestUserId);
    private readonly TimeProvider _timeProvider = new FixedTimeProvider(_fixedNow);
    private readonly GetDailyBriefQueryHandler _handler;

    public GetDailyBriefQueryTests()
    {
        _handler = new GetDailyBriefQueryHandler(_taskRepository, _profileRepository, _currentUser, _timeProvider);
        _profileRepository.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(
                TotalXp: 0, Level: 1, XpToNextLevel: 50, XpProgressPercent: 0, CurrentStreak: 4, LongestStreak: 10));
    }

    private sealed class FixedTimeProvider(DateTimeOffset fixedNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedNow;
    }

    private static TodoTask ScheduledOn(string title, DateOnly date)
    {
        return TodoTask.Create(TestUserId, new TaskTitle(title), scheduledDate: date);
    }

    private void Seed(params TodoTask[] tasks) =>
        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tasks);

    [Fact]
    public async Task Should_ReturnAvailableBrief_When_AtLeastTwoOpenTasksScheduledToday()
    {
        TodoTask today1 = ScheduledOn("Today A", Today);
        TodoTask today2 = ScheduledOn("Today B", Today);
        TodoTask later = ScheduledOn("Later", Today.AddDays(2));
        Seed(today1, today2, later);

        Result<DailyBriefReadModel> result = await _handler.Handle(new GetDailyBriefQuery(), default);

        result.IsSuccess.ShouldBeTrue();
        DailyBriefReadModel brief = result.Match(b => b, _ => throw new Xunit.Sdk.XunitException("expected success"));
        brief.Status.ShouldBe("Available");
        brief.Date.ShouldBe(Today);
        brief.CurrentStreakDays.ShouldBe(4);
        brief.CorePlanCount.ShouldBe(2);
        brief.CorePlan.Select(t => t.Title).ShouldBe(["Today A", "Today B"], ignoreOrder: true);
        brief.Overdue.ShouldBeEmpty();
        brief.IfTimeAllowsCount.ShouldBe(1);
        brief.IfTimeAllows.Single().Title.ShouldBe("Later");
        brief.Greeting.ShouldContain("Test");
    }

    [Fact]
    public async Task Should_ReturnInsufficientTasksStatus_When_FewerThanTwoCorePlanTasks()
    {
        TodoTask onlyToday = ScheduledOn("Only today", Today);
        Seed(onlyToday);

        Result<DailyBriefReadModel> result = await _handler.Handle(new GetDailyBriefQuery(), default);

        result.IsSuccess.ShouldBeTrue();
        DailyBriefReadModel brief = result.Match(b => b, _ => throw new Xunit.Sdk.XunitException("expected success"));
        brief.Status.ShouldBe("InsufficientTasks");
        brief.CorePlanCount.ShouldBe(1);
    }

    [Fact]
    public async Task Should_SurfaceOverdueSeparately_When_OpenTasksAreBeforeToday()
    {
        TodoTask overdue = ScheduledOn("Late", Today.AddDays(-2));
        TodoTask todayTask = ScheduledOn("Today", Today);
        Seed(overdue, todayTask);

        Result<DailyBriefReadModel> result = await _handler.Handle(new GetDailyBriefQuery(), default);

        DailyBriefReadModel brief = result.Match(b => b, _ => throw new Xunit.Sdk.XunitException("expected success"));
        brief.OverdueCount.ShouldBe(1);
        brief.Overdue.Single().Title.ShouldBe("Late");
        brief.CorePlanCount.ShouldBe(2);
        brief.Status.ShouldBe("Available");
    }

    [Fact]
    public async Task Should_NotLeakOtherUsersTasks_When_RepositoryScopedByCurrentUser()
    {
        // Repository port is contractually per-user (see JwtCurrentUser wiring). The handler
        // must only surface what the repository returns — we verify by seeding a narrow set
        // and confirming nothing else appears in the brief.
        TodoTask myTaskA = ScheduledOn("Mine A", Today);
        TodoTask myTaskB = ScheduledOn("Mine B", Today);
        Seed(myTaskA, myTaskB);

        Result<DailyBriefReadModel> result = await _handler.Handle(new GetDailyBriefQuery(), default);

        DailyBriefReadModel brief = result.Match(b => b, _ => throw new Xunit.Sdk.XunitException("expected success"));
        brief.CorePlan.All(t => t.Title.StartsWith("Mine", StringComparison.Ordinal)).ShouldBeTrue();
        await _taskRepository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            async () => await _handler.Handle(null!, default));
    }

    [Fact]
    public async Task Should_ExposeCalibratedMinutes_When_UserHasEnoughEstimationHistory()
    {
        // Seed three completed tasks that consistently took 50% longer than estimated.
        TodoTask history1 = CompletedWithActual(30, 45);
        TodoTask history2 = CompletedWithActual(60, 90);
        TodoTask history3 = CompletedWithActual(20, 30);

        // Plus a new open task for today with an estimate.
        TodoTask today1 = WithEstimate(ScheduledOn("Today A", Today), 40);
        TodoTask today2 = ScheduledOn("Today B", Today);
        Seed(history1, history2, history3, today1, today2);

        Result<DailyBriefReadModel> result = await _handler.Handle(new GetDailyBriefQuery(), default);

        DailyBriefReadModel brief = result.Match(b => b, _ => throw new Xunit.Sdk.XunitException("expected success"));
        DailyBriefTaskReadModel estimatedTask = brief.CorePlan.Single(t => t.Title == "Today A");
        estimatedTask.EstimatedMinutes.ShouldBe(40);
        // Bias factor = median(1.5, 1.5, 1.5) = 1.5 → 40 * 1.5 = 60.
        estimatedTask.CalibratedMinutes.ShouldBe(60);
    }

    [Fact]
    public async Task Should_ReturnNullCalibratedMinutes_When_NotEnoughEstimationHistory()
    {
        TodoTask today1 = WithEstimate(ScheduledOn("Today A", Today), 30);
        TodoTask today2 = ScheduledOn("Today B", Today);
        Seed(today1, today2);

        Result<DailyBriefReadModel> result = await _handler.Handle(new GetDailyBriefQuery(), default);

        DailyBriefReadModel brief = result.Match(b => b, _ => throw new Xunit.Sdk.XunitException("expected success"));
        DailyBriefTaskReadModel estimatedTask = brief.CorePlan.Single(t => t.Title == "Today A");
        estimatedTask.EstimatedMinutes.ShouldBe(30);
        estimatedTask.CalibratedMinutes.ShouldBeNull();
    }

    private static TodoTask WithEstimate(TodoTask task, int minutes)
    {
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(minutes));
        return task;
    }

    private static TodoTask CompletedWithActual(int estimated, int actual)
    {
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("history"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(estimated));
        task.MoveToInProgress();
        task.MarkAsDone();
        task.RecordActualTime(TimeEstimate.FromMinutes(actual));
        return task;
    }
}
