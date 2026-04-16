using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

public sealed class NotificationCreationHandlerTests
{
    private readonly INotificationRepository _repository = Substitute.For<INotificationRepository>();
    private readonly INotificationPublisher _publisher = Substitute.For<INotificationPublisher>();
    private readonly FakeCurrentUser _currentUser = new(TestUserId);
    private readonly NotificationCreationHandler _handler;

    public NotificationCreationHandlerTests()
    {
        _handler = new NotificationCreationHandler(_currentUser, _repository, _publisher);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_PushViaPublisher_After_Persisting()
    {
        await _handler.Handle(new LevelUpEvent(1, 2), CancellationToken.None);

        await _publisher.Received(1).PublishAsync(
            TestUserId,
            Arg.Is<Notification>(n =>
                n.UserId == TestUserId
                && n.Type == NotificationType.AchievementAlert),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotPublish_When_CurrentUserIsAnonymous()
    {
        NotificationCreationHandler handler = new(new AnonymousCurrentUser(), _repository, _publisher);

        await handler.Handle(new LevelUpEvent(1, 2), CancellationToken.None);

        await _publisher.DidNotReceiveWithAnyArgs().PublishAsync(default, default!, default);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_CreateAchievementNotification_When_LevelUpPublished()
    {
        // Given / When
        await _handler.Handle(new LevelUpEvent(1, 2), CancellationToken.None);

        // Then
        await _repository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.UserId == TestUserId
                && n.Type == NotificationType.AchievementAlert
                && n.Message.Contains("Level", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_CreateStreakMilestoneNotification_When_MilestoneReached()
    {
        await _handler.Handle(new StreakMilestoneReachedEvent(7, "One Week"), CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.UserId == TestUserId
                && n.Message.Contains("One Week", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_CreateQuestCompletedNotification_When_QuestCompleted()
    {
        await _handler.Handle(
            new QuestCompletedEvent(QuestId.New(), new QuestTitle("Finish MVP")),
            CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.UserId == TestUserId
                && n.Message.Contains("Finish MVP", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_Skip_When_CurrentUserIsAnonymous()
    {
        NotificationCreationHandler handler = new(new AnonymousCurrentUser(), _repository, _publisher);

        await handler.Handle(new LevelUpEvent(1, 2), CancellationToken.None);

        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_CreateFeatureUnlockNotification_When_LevelUnlocksNewFeatures()
    {
        await _handler.Handle(new LevelUpEvent(2, 3), CancellationToken.None);

        await _repository.Received().AddAsync(
            Arg.Is<Notification>(n => n.Message.Contains("SkillTrees", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotCreateFeatureUnlockNotification_When_LevelHasNoNewUnlocks()
    {
        _repository.ClearReceivedCalls();

        await _handler.Handle(new LevelUpEvent(3, 4), CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Any<Notification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_CreateMilestoneNotification_When_ReachingMilestoneLevel()
    {
        await _handler.Handle(new LevelUpEvent(9, 10), CancellationToken.None);

        await _repository.Received().AddAsync(
            Arg.Is<Notification>(n => n.Message.Contains("Double Digits", StringComparison.Ordinal)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotCreateMilestoneNotification_When_NotMilestoneLevel()
    {
        _repository.ClearReceivedCalls();

        await _handler.Handle(new LevelUpEvent(5, 6), CancellationToken.None);

        await _repository.DidNotReceive().AddAsync(
            Arg.Is<Notification>(n => n.Message.Contains("milestone", StringComparison.OrdinalIgnoreCase)),
            Arg.Any<CancellationToken>());
    }

    private sealed class AnonymousCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
        public string DisplayName => "";
        public bool IsAuthenticated => false;
    }
}
