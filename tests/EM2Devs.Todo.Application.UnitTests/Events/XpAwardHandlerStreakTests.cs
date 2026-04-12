using Shouldly;
using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Persistence;
using NSubstitute;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

[Trait("Category", "Application")]
public sealed class XpAwardHandlerStreakTests
{
    [Fact]
    public async Task Should_RecordStreakCompletionBeforeAwardingXp_When_TaskCompleted()
    {
        // Given — fresh in-memory profile
        var profileRepo = new InMemoryPlayerProfileRepository(new LastXpBreakdownCache());
        var mediator = new NoopMediator();
        var questRepo = Substitute.For<IQuestRepository>();
        questRepo.GetByTaskIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Quest>().AsReadOnly());
        var handler = new XpAwardHandler(profileRepo, mediator, questRepo);

        var taskId = TaskId.New();
        var title = new TaskTitle("Write report");
        var completedAt = new DateTimeOffset(2026, 4, 7, 9, 0, 0, TimeSpan.Zero);
        var evt = new TaskCompletedEvent(
            TaskId: taskId,
            Title: title,
            Difficulty: TaskDifficulty.Normal,
            Deadline: null,
            CompletedAt: completedAt);

        // When
        await handler.Handle(evt, CancellationToken.None);

        // Then — current streak is 1 after the first completion
        PlayerProfileReadModel profile = await profileRepo.GetProfileAsync();
        profile.CurrentStreak.ShouldBe(1);
        profile.LongestStreak.ShouldBe(1);
        profile.TotalXp.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_FeedNewStreakIntoMultiplier_When_BuildingStreak()
    {
        // Given — pre-existing 5-day streak ending yesterday (relative to the completion timestamp)
        var profileRepo = new InMemoryPlayerProfileRepository(new LastXpBreakdownCache());
        DateOnly yesterday = new(2026, 4, 6);
        for (int i = 0; i < 5; i++)
        {
            await profileRepo.RecordCompletionAsync(yesterday.AddDays(-(4 - i)));
        }
        (await profileRepo.GetProfileAsync()).CurrentStreak.ShouldBe(5);

        var mediator = new NoopMediator();
        var questRepo = Substitute.For<IQuestRepository>();
        questRepo.GetByTaskIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Quest>().AsReadOnly());
        var handler = new XpAwardHandler(profileRepo, mediator, questRepo);

        var completedAt = new DateTimeOffset(2026, 4, 7, 9, 0, 0, TimeSpan.Zero);
        var evt = new TaskCompletedEvent(
            TaskId: TaskId.New(),
            Title: new TaskTitle("Daily wrap"),
            Difficulty: TaskDifficulty.Normal,
            Deadline: null,
            CompletedAt: completedAt);

        // Capture the XP at the moment of award
        int xpBefore = (await profileRepo.GetProfileAsync()).TotalXp;

        // When
        await handler.Handle(evt, CancellationToken.None);

        // Then — streak advanced to 6, and the XP delta is non-zero
        PlayerProfileReadModel after = await profileRepo.GetProfileAsync();
        after.CurrentStreak.ShouldBe(6);
        (after.TotalXp - xpBefore).ShouldBeGreaterThan(0);

        // And the LastXpBreakdown reflects the NEW (6-day) streak multiplier, not the
        // pre-handler (5-day) one. Per XpCalculator: multiplier = 1.0 + (clamp(days, 0, 30) * 0.02).
        // 5-day pre: 1.10; 6-day post: 1.12. The test would have passed with the bug
        // (because the setup already has a 5-day streak in the repo) if we asserted
        // > 1.0 — so we assert exactly 1.12 to prove the fix actually advanced the streak
        // BEFORE the multiplier was computed.
        after.LastXpBreakdown.ShouldNotBeNull();
        after.LastXpBreakdown.StreakMultiplier.ShouldBe(1.12, 0.0001);
    }

    private sealed class NoopMediator : IMediator
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task Publish<TNotification>(TNotification notification, CancellationToken ct = default) where TNotification : INotification
            => Task.CompletedTask;
    }
}
