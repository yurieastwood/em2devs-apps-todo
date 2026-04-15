using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for Notification entity.
/// Tests encode behaviors from notifications.feature (ADR-0003).
/// </summary>
public sealed class NotificationTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateUnreadNotification_When_TaskReminderCreated()
    {
        // Given
        string message = "Submit report is due today";

        // When
        var notification = Notification.Create(NotificationType.TaskReminder, message);

        // Then
        notification.Id.Value.ShouldNotBe(Guid.Empty);
        notification.Type.ShouldBe(NotificationType.TaskReminder);
        notification.Message.ShouldBe(message);
        notification.IsRead.ShouldBeFalse();
        notification.IsDismissed.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateAchievementNotification_When_AchievementTriggered()
    {
        // Given / When
        var notification = Notification.Create(NotificationType.AchievementAlert, "Level up!");

        // Then
        notification.Type.ShouldBe(NotificationType.AchievementAlert);
        notification.Message.ShouldBe("Level up!");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BecomeRead_When_MarkedAsRead()
    {
        // Given
        var notification = Notification.Create(NotificationType.TaskReminder, "Overdue task");

        // When
        notification.MarkAsRead();

        // Then
        notification.IsRead.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDismissed_When_Dismissed()
    {
        // Given
        var notification = Notification.Create(NotificationType.AchievementAlert, "Quest completed!");

        // When
        notification.Dismiss();

        // Then
        notification.IsDismissed.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MessageIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() =>
            Notification.Create(NotificationType.TaskReminder, ""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MessageIsWhitespace()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() =>
            Notification.Create(NotificationType.TaskReminder, "   "));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveCreatedAtTimestamp_When_Created()
    {
        // Given / When
        var notification = Notification.Create(NotificationType.TaskReminder, "Test");

        // Then
        notification.CreatedAt.ShouldNotBe(default);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoAutoDismiss_When_CreatedWithoutAutoDismiss()
    {
        // Given / When
        var notification = Notification.Create(NotificationType.TaskReminder, "Test");

        // Then
        notification.AutoDismissAfterSeconds.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveAutoDismiss_When_CreatedWithAutoDismiss()
    {
        // Given / When
        var notification = Notification.Create(NotificationType.AchievementAlert, "Level up!", 5);

        // Then
        notification.AutoDismissAfterSeconds.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AutoDismissIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() =>
            Notification.Create(NotificationType.AchievementAlert, "Test", 0));
        ex.Message.ShouldContain("must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AutoDismissIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() =>
            Notification.Create(NotificationType.AchievementAlert, "Test", -1));
        ex.Message.ShouldContain("must be positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UserIdIsEmpty_ForUserNotification()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() =>
            Notification.CreateForUser(Guid.Empty, NotificationType.AchievementAlert, "Level up"));
        ex.Message.ShouldContain("UserId");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PersistUserId_When_CreatedForUser()
    {
        // Given
        Guid userId = Guid.NewGuid();

        // When
        Notification notification = Notification.CreateForUser(
            userId, NotificationType.AchievementAlert, "Level up!");

        // Then
        notification.UserId.ShouldBe(userId);
        notification.Status.ShouldBe(NotificationStatus.Unread);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionStatusAndSetReadAt_When_MarkedAsRead()
    {
        // Given
        Notification notification = Notification.CreateForUser(
            Guid.NewGuid(), NotificationType.AchievementAlert, "test");

        // When
        notification.MarkAsRead();

        // Then
        notification.Status.ShouldBe(NotificationStatus.Read);
        notification.ReadAt.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MarkingDismissedNotificationAsRead()
    {
        // Given
        Notification notification = Notification.CreateForUser(
            Guid.NewGuid(), NotificationType.AchievementAlert, "test");
        notification.Dismiss();

        // When / Then
        Should.Throw<DomainException>(() => notification.MarkAsRead());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StayEmpty_When_UsingTransientCreate()
    {
        Notification notification = Notification.Create(NotificationType.TaskReminder, "msg");
        notification.UserId.ShouldBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CreateForUserCalledWithInvalidMessage()
    {
        // Validates ValidateInputs is actually called from CreateForUser —
        // removing the call would allow empty messages through.
        Guid userId = Guid.NewGuid();
        Should.Throw<Exceptions.DomainException>(() =>
            Notification.CreateForUser(userId, NotificationType.AchievementAlert, ""));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_With_ExactMessage_When_MarkAsReadOnDismissed()
    {
        // Pins the exact error message so the string mutator can't replace it.
        Notification notification = Notification.CreateForUser(
            Guid.NewGuid(), NotificationType.AchievementAlert, "earned a title");
        notification.Dismiss();

        var ex = Should.Throw<Exceptions.DomainException>(() => notification.MarkAsRead());
        ex.Message.ShouldBe("Cannot mark a dismissed notification as read.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemainRead_When_MarkAsReadCalledTwice()
    {
        // Covers the early-return path when already Read — ReadAt should not change.
        Notification notification = Notification.CreateForUser(
            Guid.NewGuid(), NotificationType.AchievementAlert, "levelled up");
        notification.MarkAsRead();
        DateTimeOffset firstReadAt = notification.ReadAt!.Value;

        notification.MarkAsRead();

        notification.Status.ShouldBe(NotificationStatus.Read);
        notification.ReadAt.ShouldBe(firstReadAt);
    }
}
