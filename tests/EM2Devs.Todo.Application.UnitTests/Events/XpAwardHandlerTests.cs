using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

public sealed class XpAwardHandlerTests
{
    private readonly IPlayerProfileRepository _profileRepo = Substitute.For<IPlayerProfileRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly XpAwardHandler _handler;

    public XpAwardHandlerTests()
    {
        _handler = new XpAwardHandler(_profileRepo, _mediator);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AwardXp_When_TaskCompleted()
    {
        // Given
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfile(100, 2, 50, 3, 5));

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("Test"),
            TaskDifficulty.Normal);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _profileRepo.Received(1).AwardXpAsync(
            Arg.Is<ExperiencePoints>(xp => xp.Value > 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_UseDefaultDifficulty_When_NoneProvided()
    {
        // Given
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfile(0, 1, 50, 0, 0));

        TaskCompletedEvent evt = new(
            new TaskId(Guid.NewGuid()),
            new TaskTitle("Test"));

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — Normal difficulty base XP (30) should be awarded
        await _profileRepo.Received(1).AwardXpAsync(
            Arg.Is<ExperiencePoints>(xp => xp.Value == 30),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_PublishLevelUpEvent_When_XpCausesLevelUp()
    {
        // Given — profile starts at level 1, levels up after XP
        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(
                new PlayerProfile(40, 1, 10, 0, 0),  // before award
                new PlayerProfile(70, 2, 30, 0, 0));  // after award (leveled up)

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
            .Returns(new PlayerProfile(50, 2, 100, 0, 0));

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
}
