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
        var instance = recurring.GenerateNextInstance();

        // Then
        instance.ShouldNotBeNull();
        instance.Title.Value.ShouldBe("Morning standup prep");
        instance.Status.ShouldBe(TaskStatus.Todo);
        instance.Id.Value.ShouldNotBe(Guid.Empty);
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
        var first = recurring.GenerateNextInstance();
        var second = recurring.GenerateNextInstance();

        // Then
        first.Id.ShouldNotBe(second.Id);
    }
}
