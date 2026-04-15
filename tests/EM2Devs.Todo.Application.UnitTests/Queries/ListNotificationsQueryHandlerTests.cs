using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Queries;

public sealed class ListNotificationsQueryHandlerTests
{
    private readonly INotificationRepository _repository = Substitute.For<INotificationRepository>();
    private readonly ListNotificationsQueryHandler _handler;

    public ListNotificationsQueryHandlerTests()
    {
        _handler = new ListNotificationsQueryHandler(_repository);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotificationsOrderedNewestFirst()
    {
        // Given — repo returns two notifications out of chronological order
        Notification older = Notification.CreateForUser(
            TestUserId, NotificationType.AchievementAlert, "old");
        await Task.Delay(5);
        Notification newer = Notification.CreateForUser(
            TestUserId, NotificationType.AchievementAlert, "new");

        _repository.GetForCurrentUserAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new List<Notification> { older, newer }.AsReadOnly());

        // When
        Result<IReadOnlyList<Notification>> result =
            await _handler.Handle(new ListNotificationsQuery(false), CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        IReadOnlyList<Notification> list = result.Match(v => v, _ => throw new InvalidOperationException());
        list[0].Message.ShouldBe("new");
        list[1].Message.ShouldBe("old");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_PassIncludeReadFlagToRepository()
    {
        _repository.GetForCurrentUserAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new List<Notification>().AsReadOnly());

        await _handler.Handle(new ListNotificationsQuery(IncludeRead: true), CancellationToken.None);

        await _repository.Received(1).GetForCurrentUserAsync(true, Arg.Any<CancellationToken>());
    }
}
