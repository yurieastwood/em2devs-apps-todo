using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class MarkNotificationReadCommandHandlerTests
{
    private readonly INotificationRepository _repository = Substitute.For<INotificationRepository>();
    private readonly MarkNotificationReadCommandHandler _handler;

    public MarkNotificationReadCommandHandlerTests()
    {
        _handler = new MarkNotificationReadCommandHandler(_repository);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_MarkNotificationAsRead()
    {
        Notification notification = Notification.CreateForUser(
            TestUserId, NotificationType.AchievementAlert, "msg");
        _repository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        Result<Notification> result = await _handler.Handle(
            new MarkNotificationReadCommand(notification.Id.Value), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        notification.Status.ShouldBe(NotificationStatus.Read);
        await _repository.Received(1).SaveAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFound_When_NotificationMissing()
    {
        _repository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        Result<Notification> result = await _handler.Handle(
            new MarkNotificationReadCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Match<ResultError?>(_ => null, e => e).ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflict_When_AlreadyDismissed()
    {
        Notification notification = Notification.CreateForUser(
            TestUserId, NotificationType.AchievementAlert, "msg");
        notification.Dismiss();
        _repository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        Result<Notification> result = await _handler.Handle(
            new MarkNotificationReadCommand(notification.Id.Value), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Match<ResultError?>(_ => null, e => e).ShouldBeOfType<ConflictError>();
    }
}
