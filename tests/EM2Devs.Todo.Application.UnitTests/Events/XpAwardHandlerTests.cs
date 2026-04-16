using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

public sealed class XpAwardHandlerTests
{
    private readonly IPlayerProfileRepository _profileRepo = Substitute.For<IPlayerProfileRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IQuestRepository _questRepo = Substitute.For<IQuestRepository>();
    private readonly XpAwardHandler _handler;

    public XpAwardHandlerTests()
    {
        _questRepo.GetByTaskIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Quest>().AsReadOnly());
        _handler = new XpAwardHandler(_profileRepo, _mediator, _questRepo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AwardXp_When_TaskCompleted()
    {
        // Given
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(100, 2, 50, 66, 3, 5));

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("Test"),
            TaskDifficulty.Normal);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _profileRepo.Received(1).AwardXpAsync(
            Arg.Is<ExperiencePoints>(xp => xp.Value > 0),
            Arg.Any<XpBreakdownReadModel?>(),
            Arg.Any<DateOnly?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_UseDefaultDifficulty_When_NoneProvided()
    {
        // Given
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(0, 1, 50, 0, 0, 0));

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("Test"));

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — Normal difficulty base XP (30) should be awarded
        await _profileRepo.Received(1).AwardXpAsync(
            Arg.Is<ExperiencePoints>(xp => xp.Value == 30),
            Arg.Any<XpBreakdownReadModel?>(),
            Arg.Any<DateOnly?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_PublishLevelUpEvent_When_XpCausesLevelUp()
    {
        // Given — profile starts at level 1, levels up after XP
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(
                new PlayerProfileReadModel(40, 1, 10, 80, 0, 0),  // timezone read
                new PlayerProfileReadModel(40, 1, 10, 80, 0, 0),  // before award (streak read)
                new PlayerProfileReadModel(70, 2, 30, 0, 0, 0));  // after award (leveled up)

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("Level up task"),
            TaskDifficulty.Normal);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _mediator.Received(1).Publish(
            Arg.Is<LevelUpEvent>(e => e.PreviousLevel == 1 && e.NewLevel == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotPublishLevelUpEvent_When_NoLevelChange()
    {
        // Given
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(50, 2, 100, 33, 0, 0));

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("No level up"),
            TaskDifficulty.Normal);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _mediator.DidNotReceive().Publish(
            Arg.Any<LevelUpEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_UseUserTimezone_When_RecordingStreakCompletion()
    {
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(0, 1, 50, 0, 0, 0, TimeZoneId: "Australia/Sydney"));

        var completedAtUtc = new DateTimeOffset(2026, 4, 15, 12, 30, 0, TimeSpan.Zero);
        var expectedLocalDate = new DateOnly(2026, 4, 15);

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("Timezone test"),
            TaskDifficulty.Normal,
            CompletedAt: completedAtUtc);

        await _handler.Handle(evt, CancellationToken.None);

        await _profileRepo.Received(1).RecordCompletionAsync(
            Arg.Is<DateOnly>(d => d == expectedLocalDate),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_UseDifferentDate_When_UtcAndLocalDatesDiffer()
    {
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(0, 1, 50, 0, 0, 0, TimeZoneId: "Australia/Sydney"));

        var completedAtUtc = new DateTimeOffset(2026, 4, 15, 22, 0, 0, TimeSpan.Zero);
        var expectedLocalDate = new DateOnly(2026, 4, 16);

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("Cross-day test"),
            TaskDifficulty.Normal,
            CompletedAt: completedAtUtc);

        await _handler.Handle(evt, CancellationToken.None);

        await _profileRepo.Received(1).RecordCompletionAsync(
            Arg.Is<DateOnly>(d => d == expectedLocalDate),
            Arg.Any<CancellationToken>());
    }
}
