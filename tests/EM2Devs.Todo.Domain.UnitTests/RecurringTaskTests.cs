using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for RecurringTask entity.
/// Tests encode behaviors from recurring-tasks.feature (ADR-0003).
/// </summary>
public sealed class RecurringTaskTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithCorrectProperties_When_DailyRecurringTaskIsCreated()
    {
        // Given
        var title = new TaskTitle("Morning standup prep");

        // When
        var recurring = RecurringTask.Create(title, RecurrencePattern.Daily);

        // Then
        recurring.Id.Value.ShouldNotBe(Guid.Empty);
        recurring.Title.ShouldBe(title);
        recurring.Pattern.ShouldBe(RecurrencePattern.Daily);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithWeeklyPattern_When_WeeklyRecurringTaskIsCreated()
    {
        // Given
        var title = new TaskTitle("Weekly meal prep");

        // When
        var recurring = RecurringTask.Create(title, RecurrencePattern.Weekly);

        // Then
        recurring.Pattern.ShouldBe(RecurrencePattern.Weekly);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithMonthlyPattern_When_MonthlyRecurringTaskIsCreated()
    {
        // Given
        var title = new TaskTitle("Submit expense report");

        // When
        var recurring = RecurringTask.Create(title, RecurrencePattern.Monthly);

        // Then
        recurring.Pattern.ShouldBe(RecurrencePattern.Monthly);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateTodoTaskInstance_When_NextInstanceRequested()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Morning standup prep"),
            RecurrencePattern.Daily);

        // When
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, today);

        // Then
        instance.ShouldNotBeNull();
        instance.Title.Value.ShouldBe("Morning standup prep");
        instance.Status.ShouldBe(TaskStatus.Todo);
        instance.Id.Value.ShouldNotBe(Guid.Empty);
        instance.SourceRecurringTaskId.ShouldBe(recurring.Id);
        instance.ScheduledDate.ShouldBe(today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateDistinctInstances_When_MultipleInstancesRequested()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Morning standup prep"),
            RecurrencePattern.Daily);

        // When
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var first = recurring.GenerateNextInstance(TestData.TestUserId, today);
        var second = recurring.GenerateNextInstance(TestData.TestUserId, today.AddDays(1));

        // Then
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeActiveByDefault_When_Created()
    {
        // When
        RecurringTask recurring = RecurringTask.Create(
            new TaskTitle("Active task"), RecurrencePattern.Daily);

        // Then
        recurring.IsActive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BecomeInactive_When_Paused()
    {
        // Given
        RecurringTask recurring = RecurringTask.Create(
            new TaskTitle("Pausable"), RecurrencePattern.Daily);

        // When
        recurring.Pause();

        // Then
        recurring.IsActive.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BecomeActive_When_Resumed()
    {
        // Given
        RecurringTask recurring = RecurringTask.Create(
            new TaskTitle("Resumable"), RecurrencePattern.Daily);
        recurring.Pause();

        // When
        recurring.Resume();

        // Then
        recurring.IsActive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PausingAlreadyPaused()
    {
        // Given
        RecurringTask recurring = RecurringTask.Create(
            new TaskTitle("Already paused"), RecurrencePattern.Daily);
        recurring.Pause();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => recurring.Pause());
        ex.Message.ShouldContain("already paused");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ResumingAlreadyActive()
    {
        // Given
        RecurringTask recurring = RecurringTask.Create(
            new TaskTitle("Already active"), RecurrencePattern.Daily);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => recurring.Resume());
        ex.Message.ShouldContain("already active");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GeneratingFromPausedTask()
    {
        // Given
        RecurringTask recurring = RecurringTask.Create(
            new TaskTitle("Paused gen"), RecurrencePattern.Daily);
        recurring.Pause();

        // When / Then
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        DomainException ex = Should.Throw<DomainException>(() => recurring.GenerateNextInstance(TestData.TestUserId, today));
        ex.Message.ShouldContain("paused");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetDueDate_When_GeneratedFromRecurringTask()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Morning standup prep"), RecurrencePattern.Daily);
        var scheduledDate = new DateOnly(2026, 4, 1);

        // When
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, scheduledDate);

        // Then
        instance.DueDate.ShouldNotBeNull();
        instance.DueDate!.Value.Date.ShouldBe(scheduledDate.ToDateTime(TimeOnly.MinValue));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotHaveRecurringSource_When_CreatedManually()
    {
        // When
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Manual task"));

        // Then
        task.SourceRecurringTaskId.ShouldBeNull();
        task.ScheduledDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MarkAsSkipped_When_SkipCalled()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Skippable"), RecurrencePattern.Daily);
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, DateOnly.FromDateTime(DateTime.UtcNow));

        // When
        instance.Skip();

        // Then
        instance.Status.ShouldBe(TaskStatus.Skipped);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SkippingCompletedTask()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Done task"));
        task.MoveToInProgress();
        task.MarkAsDone();

        // When / Then
        var ex = Should.Throw<DomainException>(() => task.Skip());
        ex.Message.ShouldContain("Cannot skip a completed task");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SkippingAlreadySkippedTask()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Skip twice"));
        task.Skip();

        // When / Then
        var ex = Should.Throw<DomainException>(() => task.Skip());
        ex.Message.ShouldContain("already skipped");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeOverdue_When_Skipped()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Overdue check"), RecurrencePattern.Daily);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, yesterday);

        // When
        instance.Skip();

        // Then
        instance.IsOverdue.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeOverdue_When_ScheduledDateIsPastAndStatusIsTodo()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Overdue test"), RecurrencePattern.Daily);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        TodoTask instance = recurring.GenerateNextInstance(TestData.TestUserId, yesterday);

        // Then — status is Todo, scheduled date is past → overdue
        instance.IsOverdue.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeOverdue_When_ScheduledDateIsToday()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Today test"), RecurrencePattern.Daily);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        TodoTask instance = recurring.GenerateNextInstance(TestData.TestUserId, today);

        // Then — scheduled date is today, not past → not overdue
        instance.IsOverdue.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeOverdue_When_ScheduledDateIsFuture()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Future test"), RecurrencePattern.Daily);
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        TodoTask instance = recurring.GenerateNextInstance(TestData.TestUserId, tomorrow);

        // Then
        instance.IsOverdue.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeOverdue_When_TaskIsDone()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Done test"), RecurrencePattern.Daily);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        TodoTask instance = recurring.GenerateNextInstance(TestData.TestUserId, yesterday);
        instance.MoveToInProgress();
        instance.MarkAsDone();

        // Then — completed tasks are never overdue
        instance.IsOverdue.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeOverdue_When_NoScheduledDate()
    {
        // Given — manually created task with no scheduled date
        TodoTask task = TodoTask.Create(TestData.TestUserId, new TaskTitle("No schedule"));

        // Then
        task.IsOverdue.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreateFromRecurringCalledWithNullSourceId()
    {
        // When / Then
        Should.Throw<ArgumentNullException>(() =>
            TodoTask.CreateFromRecurring(TestData.TestUserId, new TaskTitle("Test"), null!, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RecurringTaskUpdateTitleCalledWithNull()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Null check"), RecurrencePattern.Daily);

        // When / Then
        Should.Throw<ArgumentNullException>(() => recurring.UpdateTitle(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateTitle_When_UpdateTitleCalled()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Old title"), RecurrencePattern.Daily);

        // When
        recurring.UpdateTitle(new TaskTitle("New title"));

        // Then
        recurring.Title.Value.ShouldBe("New title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdatePattern_When_UpdatePatternCalled()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Pattern change"), RecurrencePattern.Daily);

        // When
        recurring.UpdatePattern(RecurrencePattern.Weekly);

        // Then
        recurring.Pattern.ShouldBe(RecurrencePattern.Weekly);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UpdatePatternCalledWithInvalidValue()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Invalid pattern"), RecurrencePattern.Daily);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => recurring.UpdatePattern((RecurrencePattern)999));
        ex.Message.ShouldContain("Invalid recurrence pattern");
    }

    // --- Scenario: Create a recurring task with an end date ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StoreEndDate_When_CreatedWithEndDate()
    {
        // Given
        var endDate = new DateOnly(2026, 6, 30);

        // When
        var recurring = RecurringTask.Create(
            new TaskTitle("Sprint retrospective"),
            RecurrencePattern.Weekly,
            endDate);

        // Then
        recurring.EndDate.ShouldBe(endDate);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullEndDate_When_CreatedWithoutEndDate()
    {
        // When
        var recurring = RecurringTask.Create(
            new TaskTitle("Endless task"),
            RecurrencePattern.Daily);

        // Then
        recurring.EndDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateInstance_When_ScheduledDateIsBeforeEndDate()
    {
        // Given
        var endDate = new DateOnly(2026, 6, 30);
        var recurring = RecurringTask.Create(
            new TaskTitle("Sprint retrospective"),
            RecurrencePattern.Weekly,
            endDate);

        // When
        var scheduledDate = new DateOnly(2026, 6, 29);
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, scheduledDate);

        // Then
        instance.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateInstance_When_ScheduledDateEqualsEndDate()
    {
        // Given
        var endDate = new DateOnly(2026, 6, 30);
        var recurring = RecurringTask.Create(
            new TaskTitle("Sprint retrospective"),
            RecurrencePattern.Weekly,
            endDate);

        // When
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, endDate);

        // Then
        instance.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ScheduledDateIsAfterEndDate()
    {
        // Given
        var endDate = new DateOnly(2026, 6, 30);
        var recurring = RecurringTask.Create(
            new TaskTitle("Sprint retrospective"),
            RecurrencePattern.Weekly,
            endDate);

        // When / Then
        var scheduledDate = new DateOnly(2026, 7, 1);
        var ex = Should.Throw<DomainException>(() => recurring.GenerateNextInstance(TestData.TestUserId, scheduledDate));
        ex.Message.ShouldContain("end date");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndDateIsInThePast()
    {
        // Given
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);

        // When / Then
        var ex = Should.Throw<DomainException>(() =>
            RecurringTask.Create(
                new TaskTitle("Past end date"),
                RecurrencePattern.Daily,
                pastDate));
        ex.Message.ShouldContain("end date");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowCreation_When_EndDateIsToday()
    {
        // Given — end date is today (boundary: should NOT throw)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // When
        var recurring = RecurringTask.Create(
            new TaskTitle("Today end date"),
            RecurrencePattern.Daily,
            today);

        // Then
        recurring.EndDate.ShouldBe(today);
    }

    // --- Scenario: Complete a recurring task instance late ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeCompletedLate_When_CompletedAfterScheduledDate()
    {
        // Given — a recurring task instance scheduled for yesterday
        var recurring = RecurringTask.Create(
            new TaskTitle("Morning standup prep"),
            RecurrencePattern.Daily);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, yesterday);

        // When — complete it today (late)
        instance.MoveToInProgress();
        instance.MarkAsDone();

        // Then — it should be flagged as completed late
        instance.WasCompletedLate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeCompletedLate_When_CompletedOnScheduledDate()
    {
        // Given — a recurring task instance scheduled for today
        var recurring = RecurringTask.Create(
            new TaskTitle("Morning standup prep"),
            RecurrencePattern.Daily);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, today);

        // When — complete it today (on time)
        instance.MoveToInProgress();
        instance.MarkAsDone();

        // Then
        instance.WasCompletedLate.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeCompletedLate_When_ManualTaskCompleted()
    {
        // Given — a manual task with no scheduled date
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Manual task"));

        // When
        task.MoveToInProgress();
        task.MarkAsDone();

        // Then — no scheduled date means not late
        task.WasCompletedLate.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeCompletedLate_When_NotYetCompleted()
    {
        // Given
        var recurring = RecurringTask.Create(
            new TaskTitle("Morning standup prep"),
            RecurrencePattern.Daily);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var instance = recurring.GenerateNextInstance(TestData.TestUserId, yesterday);

        // Then — not completed yet, so WasCompletedLate is false
        instance.WasCompletedLate.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BreakStreak_When_RecurringTaskCompletedLate()
    {
        // Given — a streak with 5 consecutive days
        var streak = new Streak(5, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), 0);

        // When — late completion resets the streak
        var broken = streak.BreakStreak();

        // Then
        broken.CurrentDays.ShouldBe(0);
    }
}
