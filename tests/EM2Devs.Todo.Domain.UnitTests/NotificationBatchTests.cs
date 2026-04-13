using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for NotificationBatch: batch notifications when many arrive simultaneously.
/// </summary>
public sealed class NotificationBatchTests
{
    private static Notification MakeNotification(NotificationType type = NotificationType.AchievementAlert, string message = "Achievement unlocked")
    {
        return Notification.Create(type, message);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateBatch_From_TwoOrMoreNotifications()
    {
        List<Notification> notifications = Enumerable.Range(0, 5)
            .Select(_ => MakeNotification())
            .ToList();

        NotificationBatch batch = NotificationBatch.Create(notifications);
        batch.Count.ShouldBe(5);
        batch.Type.ShouldBe(NotificationType.AchievementAlert);
        batch.Summary.ShouldContain("5");
        batch.Notifications.Count.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_LessThanMinimumBatchSize()
    {
        List<Notification> notifications = new List<Notification> { MakeNotification() };
        DomainException ex = Should.Throw<DomainException>(() => NotificationBatch.Create(notifications));
        ex.Message.ShouldContain("requires at least");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_MixedTypes()
    {
        List<Notification> notifications = new List<Notification>
        {
            MakeNotification(NotificationType.AchievementAlert),
            MakeNotification(NotificationType.TaskReminder, "Reminder"),
        };
        DomainException ex = Should.Throw<DomainException>(() => NotificationBatch.Create(notifications));
        ex.Message.ShouldContain("same type");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptExactlyMinimumBatchSize()
    {
        // Kills `< MinimumBatchSize` -> `<= MinimumBatchSize` mutation in both Create and TryCreate.
        List<Notification> notifications = new List<Notification>
        {
            MakeNotification(), MakeNotification(),
        };
        NotificationBatch batch = NotificationBatch.Create(notifications);
        batch.Count.ShouldBe(2);

        NotificationBatch? tryBatch = NotificationBatch.TryCreate(notifications, TimeSpan.FromSeconds(10));
        tryBatch.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptWindowExactlyAtBoundary()
    {
        // Kills `> window` -> `>= window` mutation.
        // Two notifications with a tiny gap between them, provide window equal to that gap.
        Notification n1 = Notification.Create(NotificationType.AchievementAlert, "one");
        Notification n2 = Notification.Create(NotificationType.AchievementAlert, "two");
        TimeSpan actualGap = n2.CreatedAt - n1.CreatedAt;
        NotificationBatch? batch = NotificationBatch.TryCreate(new List<Notification> { n1, n2 }, actualGap);
        batch.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowNullArgument_When_NotificationsNull()
    {
        Should.Throw<ArgumentNullException>(() => NotificationBatch.Create(null!));
        Should.Throw<ArgumentNullException>(() => NotificationBatch.TryCreate(null!, TimeSpan.FromSeconds(10)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TryCreate_ReturnNull_When_BelowMinimum()
    {
        List<Notification> notifications = new List<Notification> { MakeNotification() };
        NotificationBatch? batch = NotificationBatch.TryCreate(notifications, TimeSpan.FromSeconds(10));
        batch.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TryCreate_ReturnBatch_When_WithinWindow()
    {
        List<Notification> notifications = Enumerable.Range(0, 3)
            .Select(_ => MakeNotification())
            .ToList();
        NotificationBatch? batch = NotificationBatch.TryCreate(notifications, TimeSpan.FromSeconds(10));
        batch.ShouldNotBeNull();
        batch!.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TryCreate_ReturnNull_When_OutsideWindow()
    {
        // Create several notifications with a small delay so their CreatedAt differs.
        List<Notification> notifications = new List<Notification>();
        for (int i = 0; i < 3; i++)
        {
            notifications.Add(MakeNotification());
            System.Threading.Thread.Sleep(5);
        }

        NotificationBatch? batch = NotificationBatch.TryCreate(notifications, TimeSpan.Zero);
        batch.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeDefaultBatchWindow()
    {
        NotificationBatch.DefaultBatchWindow.ShouldBe(TimeSpan.FromSeconds(10));
        NotificationBatch.MinimumBatchSize.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SummariseUsingCountAndType()
    {
        List<Notification> notifications = Enumerable.Range(0, 2)
            .Select(_ => MakeNotification(NotificationType.TaskReminder, "Reminder"))
            .ToList();
        NotificationBatch batch = NotificationBatch.Create(notifications);
        batch.Summary.ShouldContain("TaskReminder");
        batch.Summary.ShouldContain("2");
    }
}
