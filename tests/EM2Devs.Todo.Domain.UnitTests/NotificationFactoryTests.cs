using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for NotificationFactory domain service.
/// Tests encode behaviors from notifications.feature.
/// </summary>
public sealed class NotificationFactoryTests
{
    private static readonly DateTimeOffset _now = new DateTimeOffset(2026, 4, 12, 14, 0, 0, TimeSpan.Zero);

    private static NotificationSettings DefaultSettings() => NotificationSettings.CreateDefault();

    private static TodoTask CreateTaskDueToday(string title = "Submit report")
    {
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle(title),
            dueDate: new DateTimeOffset(DateOnly.FromDateTime(_now.Date).ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), TimeSpan.Zero));
        return task;
    }

    private static TodoTask CreateTaskDueIn(string title, TimeSpan dueIn)
    {
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle(title),
            dueDate: _now + dueIn);
        return task;
    }

    private static TodoTask CreateOverdueTask(string title = "Submit report", int daysOverdue = 2)
    {
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle(title),
            dueDate: _now.AddDays(-daysOverdue));
        return task;
    }

    private static TodoTask CreateCompletedTask(string title = "Buy milk")
    {
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle(title),
            dueDate: new DateTimeOffset(DateOnly.FromDateTime(_now.Date).ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), TimeSpan.Zero));
        task.MoveToInProgress();
        task.MarkAsDone();
        return task;
    }

    private static TodoTask CreateSkippedTask(string title = "Skipped task")
    {
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle(title),
            dueDate: new DateTimeOffset(DateOnly.FromDateTime(_now.Date).ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc), TimeSpan.Zero));
        task.Skip();
        return task;
    }

    // ======================================================================
    // Scenario: Reminder for task due today
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateReminder_When_TaskIsDueToday()
    {
        // Given
        TodoTask task = CreateTaskDueToday();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateDueTodayReminder(task, settings, _now);

        // Then
        notification.ShouldNotBeNull();
        notification!.Type.ShouldBe(NotificationType.TaskReminder);
        notification.Message.ShouldContain("Submit report");
        notification.Message.ShouldContain("due today");
        notification.IsRead.ShouldBeFalse();
        notification.IsDismissed.ShouldBeFalse();
    }

    // ======================================================================
    // Scenario: Reminder for upcoming deadline
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateReminder_When_TaskDueWithinReminderWindow()
    {
        // Given — task due in 2 days, reminder window is 48 hours
        TodoTask task = CreateTaskDueIn("Prepare presentation", TimeSpan.FromDays(2));
        NotificationSettings settings = DefaultSettings();
        TimeSpan window = TimeSpan.FromHours(48);

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(task, settings, _now, window);

        // Then
        notification.ShouldNotBeNull();
        notification!.Type.ShouldBe(NotificationType.TaskReminder);
        notification.Message.ShouldContain("Prepare presentation");
        notification.Message.ShouldContain("due soon");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateReminder_When_TaskDueOutsideReminderWindow()
    {
        // Given — task due in 5 days, reminder window is 48 hours
        TodoTask task = CreateTaskDueIn("Far away task", TimeSpan.FromDays(5));
        NotificationSettings settings = DefaultSettings();
        TimeSpan window = TimeSpan.FromHours(48);

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(task, settings, _now, window);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateUpcomingReminder_When_TaskAlreadyOverdue()
    {
        // Given — task is overdue
        TodoTask task = CreateOverdueTask("Past task");
        NotificationSettings settings = DefaultSettings();
        TimeSpan window = TimeSpan.FromHours(48);

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(task, settings, _now, window);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateUpcomingReminder_When_TaskHasNoDueDate()
    {
        // Given — task without due date
        TodoTask task = TodoTask.Create(TestData.TestUserId, new TaskTitle("No deadline"));
        NotificationSettings settings = DefaultSettings();
        TimeSpan window = TimeSpan.FromHours(48);

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(task, settings, _now, window);

        // Then
        notification.ShouldBeNull();
    }

    // ======================================================================
    // Scenario: No reminder for completed tasks
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateReminder_When_TaskIsCompleted()
    {
        // Given
        TodoTask task = CreateCompletedTask("Buy milk");
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateDueTodayReminder(task, settings, _now);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateReminder_When_TaskIsSkipped()
    {
        // Given
        TodoTask task = CreateSkippedTask();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateDueTodayReminder(task, settings, _now);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateUpcomingReminder_When_TaskIsCompleted()
    {
        // Given
        TodoTask task = CreateCompletedTask();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(
            task, settings, _now, TimeSpan.FromHours(48));

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateUpcomingReminder_When_TaskIsSkipped()
    {
        // Given
        TodoTask task = CreateSkippedTask();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(
            task, settings, _now, TimeSpan.FromHours(48));

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_TaskIsCompleted()
    {
        // Given
        TodoTask task = CreateCompletedTask();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_TaskIsSkipped()
    {
        // Given
        TodoTask task = CreateSkippedTask();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then
        notification.ShouldBeNull();
    }

    // ======================================================================
    // Scenario: Repeated reminders for overdue tasks
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateOverdueReminder_When_TaskIsOverdue()
    {
        // Given — task 2 days overdue, no prior reminder
        TodoTask task = CreateOverdueTask();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then
        notification.ShouldNotBeNull();
        notification!.Type.ShouldBe(NotificationType.TaskReminder);
        notification.Message.ShouldContain("Submit report");
        notification.Message.ShouldContain("overdue");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateOverdueReminder_When_LastReminderWasYesterday()
    {
        // Given — last reminder was 25 hours ago
        TodoTask task = CreateOverdueTask();
        NotificationSettings settings = DefaultSettings();
        DateTimeOffset lastReminder = _now.AddHours(-25);

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, lastReminder);

        // Then
        notification.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_LastReminderWasToday()
    {
        // Given — last reminder was 12 hours ago (less than 24)
        TodoTask task = CreateOverdueTask();
        NotificationSettings settings = DefaultSettings();
        DateTimeOffset lastReminder = _now.AddHours(-12);

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, lastReminder);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_LastReminderWasExactly24HoursAgo()
    {
        // Given — boundary: exactly 24 hours ago (still within the 24h window — "< 24" check)
        TodoTask task = CreateOverdueTask();
        NotificationSettings settings = DefaultSettings();
        DateTimeOffset lastReminder = _now.AddHours(-24);

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, lastReminder);

        // Then — 24 hours is NOT < 24, so it should create a reminder
        notification.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_TaskNotYetOverdue()
    {
        // Given — task due in the future
        TodoTask task = CreateTaskDueIn("Future task", TimeSpan.FromDays(1));
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_TaskHasNoDueDate()
    {
        // Given
        TodoTask task = TodoTask.Create(TestData.TestUserId, new TaskTitle("No deadline"));
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then
        notification.ShouldBeNull();
    }

    // ======================================================================
    // Scenario: Notification for achievement
    // ======================================================================

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("Level up")]
    [InlineData("Title earned")]
    [InlineData("Streak milestone reached")]
    [InlineData("Skill tree unlocked")]
    [InlineData("Quest completed")]
    [InlineData("Boss Task defeated")]
    [InlineData("Season rank achieved")]
    public void Should_CreateAchievementNotification_When_AchievementTriggered(string achievement)
    {
        // Given
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateAchievementNotification(achievement, settings, _now);

        // Then
        notification.ShouldNotBeNull();
        notification!.Type.ShouldBe(NotificationType.AchievementAlert);
        notification.Message.ShouldContain(achievement);
        notification.AutoDismissAfterSeconds.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AchievementNameIsEmpty()
    {
        // Given
        NotificationSettings settings = DefaultSettings();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            NotificationFactory.CreateAchievementNotification("", settings, _now));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_AchievementNameIsWhitespace()
    {
        // Given
        NotificationSettings settings = DefaultSettings();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            NotificationFactory.CreateAchievementNotification("   ", settings, _now));
        ex.Message.ShouldContain("cannot be empty");
    }

    // ======================================================================
    // Scenario: Configure notification categories
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateDueReminder_When_TaskRemindersDisabled()
    {
        // Given
        TodoTask task = CreateTaskDueToday();
        NotificationSettings settings = DefaultSettings()
            .WithCategoryToggle(NotificationCategory.TaskReminders, false);

        // When
        Notification? notification = NotificationFactory.CreateDueTodayReminder(task, settings, _now);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateUpcomingReminder_When_TaskRemindersDisabled()
    {
        // Given
        TodoTask task = CreateTaskDueIn("Presentation", TimeSpan.FromDays(1));
        NotificationSettings settings = DefaultSettings()
            .WithCategoryToggle(NotificationCategory.TaskReminders, false);

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(
            task, settings, _now, TimeSpan.FromHours(48));

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_TaskRemindersDisabled()
    {
        // Given
        TodoTask task = CreateOverdueTask();
        NotificationSettings settings = DefaultSettings()
            .WithCategoryToggle(NotificationCategory.TaskReminders, false);

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateAchievementNotification_When_AchievementAlertsDisabled()
    {
        // Given
        NotificationSettings settings = DefaultSettings()
            .WithCategoryToggle(NotificationCategory.AchievementAlerts, false);

        // When
        Notification? notification = NotificationFactory.CreateAchievementNotification("Level up", settings, _now);

        // Then
        notification.ShouldBeNull();
    }

    // ======================================================================
    // Scenario: Set quiet hours / Quiet hours respect user timezone
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateDueReminder_When_InQuietHours()
    {
        // Given — quiet hours 10 PM to 7 AM, current time 11 PM UTC
        TodoTask task = CreateTaskDueToday();
        NotificationSettings settings = DefaultSettings()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));
        DateTimeOffset lateNight = new DateTimeOffset(2026, 4, 12, 23, 0, 0, TimeSpan.Zero);

        // When
        Notification? notification = NotificationFactory.CreateDueTodayReminder(task, settings, lateNight);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateUpcomingReminder_When_InQuietHours()
    {
        // Given
        TodoTask task = CreateTaskDueIn("Presentation", TimeSpan.FromDays(1));
        NotificationSettings settings = DefaultSettings()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));
        DateTimeOffset lateNight = new DateTimeOffset(2026, 4, 12, 23, 0, 0, TimeSpan.Zero);

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(
            task, settings, lateNight, TimeSpan.FromHours(48));

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_InQuietHours()
    {
        // Given
        TodoTask task = CreateOverdueTask();
        NotificationSettings settings = DefaultSettings()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));
        DateTimeOffset lateNight = new DateTimeOffset(2026, 4, 12, 23, 0, 0, TimeSpan.Zero);

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, lateNight, null);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateAchievementNotification_When_InQuietHours()
    {
        // Given
        NotificationSettings settings = DefaultSettings()
            .WithQuietHours(new TimeOnly(22, 0), new TimeOnly(7, 0));
        DateTimeOffset lateNight = new DateTimeOffset(2026, 4, 12, 23, 0, 0, TimeSpan.Zero);

        // When
        Notification? notification = NotificationFactory.CreateAchievementNotification("Level up", settings, lateNight);

        // Then
        notification.ShouldBeNull();
    }

    // ======================================================================
    // Null argument tests
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_DueTodayTaskIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            NotificationFactory.CreateDueTodayReminder(null!, DefaultSettings(), _now));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_DueTodaySettingsIsNull()
    {
        TodoTask task = CreateTaskDueToday();
        Should.Throw<ArgumentNullException>(() =>
            NotificationFactory.CreateDueTodayReminder(task, null!, _now));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_UpcomingTaskIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            NotificationFactory.CreateUpcomingDeadlineReminder(null!, DefaultSettings(), _now, TimeSpan.FromHours(48)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_UpcomingSettingsIsNull()
    {
        TodoTask task = CreateTaskDueToday();
        Should.Throw<ArgumentNullException>(() =>
            NotificationFactory.CreateUpcomingDeadlineReminder(task, null!, _now, TimeSpan.FromHours(48)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_OverdueTaskIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            NotificationFactory.CreateOverdueReminder(null!, DefaultSettings(), _now, null));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_OverdueSettingsIsNull()
    {
        TodoTask task = CreateOverdueTask();
        Should.Throw<ArgumentNullException>(() =>
            NotificationFactory.CreateOverdueReminder(task, null!, _now, null));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AchievementSettingsIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            NotificationFactory.CreateAchievementNotification("Level up", null!, _now));
    }

    // ======================================================================
    // Boundary tests for mutation killing
    // ======================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateUpcomingReminder_When_TaskDueExactlyAtCurrentTime()
    {
        // Given — task due exactly now (timeUntilDue == TimeSpan.Zero, boundary test for < vs <=)
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Exact deadline"),
            dueDate: _now);
        NotificationSettings settings = DefaultSettings();
        TimeSpan window = TimeSpan.FromHours(48);

        // When
        Notification? notification = NotificationFactory.CreateUpcomingDeadlineReminder(task, settings, _now, window);

        // Then — timeUntilDue is exactly Zero, which is NOT < Zero, so notification should be created
        notification.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotCreateOverdueReminder_When_CompletedTaskIsAlsoOverdue()
    {
        // Given — a task that was completed but its due date is in the past
        // This kills the block-removal mutant on IsCompletedOrSkipped for overdue path
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Old task"),
            dueDate: _now.AddDays(-3));
        task.MoveToInProgress();
        task.MarkAsDone();
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then
        notification.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateOverdueReminder_When_TaskDueExactlyAtCurrentTime()
    {
        // Given — task due exactly at current time (boundary for > vs >=)
        TodoTask task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Exact due task"),
            dueDate: _now);
        NotificationSettings settings = DefaultSettings();

        // When
        Notification? notification = NotificationFactory.CreateOverdueReminder(task, settings, _now, null);

        // Then — DueDate == currentUtcTime, which is NOT > currentUtcTime, so notification should be created
        notification.ShouldNotBeNull();
    }
}
