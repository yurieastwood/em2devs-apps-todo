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
    private readonly FakeCurrentUser _currentUser = new(TestUserId);
    private readonly NotificationCreationHandler _handler;

    public NotificationCreationHandlerTests()
    {
        _handler = new NotificationCreationHandler(_currentUser, _repository);
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
        NotificationCreationHandler handler = new(new AnonymousCurrentUser(), _repository);

        await handler.Handle(new LevelUpEvent(1, 2), CancellationToken.None);

        await _repository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    private sealed class AnonymousCurrentUser : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
        public string DisplayName => "";
        public bool IsAuthenticated => false;
    }
}
