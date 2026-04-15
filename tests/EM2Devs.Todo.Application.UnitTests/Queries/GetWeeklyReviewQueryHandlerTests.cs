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
public sealed class GetWeeklyReviewQueryHandlerTests
{
    // Wednesday 2026-04-15 at 10:00 UTC — Sunday of same week is 2026-04-12.
    private static readonly DateTimeOffset _fixedNow = new(2026, 4, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly _weekOf = new(2026, 4, 12);

    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly IPlayerProfileRepository _profileRepository = Substitute.For<IPlayerProfileRepository>();
    private readonly IWeeklyReflectionRepository _reflectionRepository = Substitute.For<IWeeklyReflectionRepository>();
    private readonly TimeProvider _timeProvider = new FixedTimeProvider(_fixedNow);
    private readonly GetWeeklyReviewQueryHandler _handler;

    public GetWeeklyReviewQueryHandlerTests()
    {
        _handler = new GetWeeklyReviewQueryHandler(
            _taskRepository, _profileRepository, _reflectionRepository, _timeProvider);
        _profileRepository.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(
                TotalXp: 0, Level: 1, XpToNextLevel: 50, CurrentStreak: 5, LongestStreak: 10));
        _reflectionRepository.GetAsync(Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns((WeeklyReflectionReadModel?)null);
        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TodoTask>());
    }

    private sealed class FixedTimeProvider(DateTimeOffset fixedNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => fixedNow;
    }

    private static TodoTask CompletedAt(string title, DateTimeOffset completedAt)
    {
        TodoTask t = TodoTask.Create(TestUserId, new TaskTitle(title));
        // CompletedAt has a private setter; use reflection to inject a deterministic
        // completion time so we can test week-range filtering independently of wall clock.
        typeof(TodoTask)
            .GetProperty(nameof(TodoTask.CompletedAt))!
            .SetValue(t, completedAt);
        return t;
    }

    [Fact]
    public async Task Should_ComputeWeekOfSunday_When_WeekOfOmitted()
    {
        Result<WeeklyReviewReadModel> result = await _handler.Handle(new GetWeeklyReviewQuery(), default);

        result.IsSuccess.ShouldBeTrue();
        WeeklyReviewReadModel model = result.Match(m => m, _ => throw new Xunit.Sdk.XunitException("expected success"));
        model.WeekOf.ShouldBe(_weekOf);
    }

    [Fact]
    public async Task Should_CountOnlyTasksCompletedInTheWeek()
    {
        TodoTask inside = CompletedAt("In", new DateTimeOffset(2026, 4, 13, 8, 0, 0, TimeSpan.Zero));
        TodoTask alsoInside = CompletedAt("Also", new DateTimeOffset(2026, 4, 18, 23, 0, 0, TimeSpan.Zero));
        TodoTask outside = CompletedAt("Out", new DateTimeOffset(2026, 4, 11, 23, 0, 0, TimeSpan.Zero));
        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([inside, alsoInside, outside]);

        Result<WeeklyReviewReadModel> result = await _handler.Handle(new GetWeeklyReviewQuery(), default);

        WeeklyReviewReadModel model = result.Match(m => m, _ => throw new Xunit.Sdk.XunitException("expected success"));
        model.TasksCompleted.ShouldBe(2);
    }

    [Fact]
    public async Task Should_SumXpEarnedFromHistoryInsideWeek()
    {
        _profileRepository.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(
                TotalXp: 500, Level: 2, XpToNextLevel: 50, CurrentStreak: 7, LongestStreak: 10,
                XpHistory: new List<XpHistoryEntryReadModel>
                {
                    new(new DateOnly(2026, 4, 13), 100, "TaskCompletion", 100),
                    new(new DateOnly(2026, 4, 14), 50, "TaskCompletion", 150),
                    new(new DateOnly(2026, 4, 11), 999, "TaskCompletion", 1149), // outside, ignored
                }));

        Result<WeeklyReviewReadModel> result = await _handler.Handle(new GetWeeklyReviewQuery(), default);

        WeeklyReviewReadModel model = result.Match(m => m, _ => throw new Xunit.Sdk.XunitException("expected success"));
        model.XpEarned.ShouldBe(150);
        model.NotableEvents.ShouldContain(e => e.Contains("100 XP"));
        model.NotableEvents.ShouldContain(e => e.Contains("50 XP"));
    }

    [Fact]
    public async Task Should_ReturnPreviouslySavedReflection_When_PresentForWeek()
    {
        WeeklyReflectionReadModel saved = new(
            "went well", "dragged", "adjust", new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.Zero));
        _reflectionRepository.GetAsync(_weekOf, Arg.Any<CancellationToken>()).Returns(saved);

        Result<WeeklyReviewReadModel> result = await _handler.Handle(new GetWeeklyReviewQuery(), default);

        WeeklyReviewReadModel model = result.Match(m => m, _ => throw new Xunit.Sdk.XunitException("expected success"));
        model.Reflection.ShouldBe(saved);
    }

    [Fact]
    public async Task Should_UseProvidedWeekOf_When_Supplied()
    {
        DateOnly explicitWeek = new(2026, 3, 29); // Sunday
        Result<WeeklyReviewReadModel> result = await _handler.Handle(new GetWeeklyReviewQuery(explicitWeek), default);

        WeeklyReviewReadModel model = result.Match(m => m, _ => throw new Xunit.Sdk.XunitException("expected success"));
        model.WeekOf.ShouldBe(explicitWeek);
        await _reflectionRepository.Received(1).GetAsync(explicitWeek, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Should_ReturnSundayItself_When_CalledWithSunday()
    {
        DateOnly sunday = new(2026, 4, 12);
        GetWeeklyReviewQueryHandler.GetWeekOfSunday(sunday).ShouldBe(sunday);
    }

    [Fact]
    public void Should_ReturnPriorSunday_When_CalledWithSaturday()
    {
        DateOnly saturday = new(2026, 4, 18);
        GetWeeklyReviewQueryHandler.GetWeekOfSunday(saturday).ShouldBe(new DateOnly(2026, 4, 12));
    }

    [Fact]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(async () => await _handler.Handle(null!, default));
    }
}
