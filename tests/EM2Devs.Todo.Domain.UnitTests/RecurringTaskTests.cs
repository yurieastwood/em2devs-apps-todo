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
        var instance = recurring.GenerateNextInstance(today);

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
        var first = recurring.GenerateNextInstance(today);
        var second = recurring.GenerateNextInstance(today.AddDays(1));

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
        DomainException ex = Should.Throw<DomainException>(() => recurring.GenerateNextInstance(today));
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
        var instance = recurring.GenerateNextInstance(scheduledDate);

        // Then
        instance.DueDate.ShouldNotBeNull();
        instance.DueDate!.Value.Date.ShouldBe(scheduledDate.ToDateTime(TimeOnly.MinValue));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotHaveRecurringSource_When_CreatedManually()
    {
        // When
        var task = TodoTask.Create(new TaskTitle("Manual task"));

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
        var instance = recurring.GenerateNextInstance(DateOnly.FromDateTime(DateTime.UtcNow));

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
        var task = TodoTask.Create(new TaskTitle("Done task"));
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
        var task = TodoTask.Create(new TaskTitle("Skip twice"));
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
        var instance = recurring.GenerateNextInstance(yesterday);

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
        TodoTask instance = recurring.GenerateNextInstance(yesterday);

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
        TodoTask instance = recurring.GenerateNextInstance(today);

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
        TodoTask instance = recurring.GenerateNextInstance(tomorrow);

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
        TodoTask instance = recurring.GenerateNextInstance(yesterday);
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
        TodoTask task = TodoTask.Create(new TaskTitle("No schedule"));

        // Then
        task.IsOverdue.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreateFromRecurringCalledWithNullSourceId()
    {
        // When / Then
        Should.Throw<ArgumentNullException>(() =>
            TodoTask.CreateFromRecurring(new TaskTitle("Test"), null!, DateOnly.FromDateTime(DateTime.UtcNow)));
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
}
