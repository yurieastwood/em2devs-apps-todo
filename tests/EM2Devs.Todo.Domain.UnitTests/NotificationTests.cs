using Shouldly;
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
}
