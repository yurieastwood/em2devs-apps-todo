using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Tests for RecurringTask.IsDueForGeneration(today) and MarkInstanceGenerated(today).
/// Maps to: docs/features/core/recurring-tasks.feature
/// </summary>
public sealed class RecurringTaskGenerationDueTests
{
    private static readonly TaskTitle _title = new("Daily standup");
    private static readonly DateOnly _today = new(2026, 4, 7); // Tuesday

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_NeverGenerated()
    {
        // Given
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Daily);

        // Then
        recurring.IsDueForGeneration(_today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_DailyAlreadyGeneratedToday()
    {
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Daily);
        recurring.MarkInstanceGenerated(_today);

        recurring.IsDueForGeneration(_today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_DailyLastGeneratedYesterday()
    {
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Daily);
        recurring.MarkInstanceGenerated(_today.AddDays(-1));

        recurring.IsDueForGeneration(_today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_WeeklyLastGenerated6DaysAgo()
    {
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Weekly);
        recurring.MarkInstanceGenerated(_today.AddDays(-6));

        recurring.IsDueForGeneration(_today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_WeeklyLastGenerated7DaysAgo()
    {
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Weekly);
        recurring.MarkInstanceGenerated(_today.AddDays(-7));

        recurring.IsDueForGeneration(_today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_MonthlyLastGeneratedSameMonth()
    {
        // Given — both dates in April 2026
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Monthly);
        recurring.MarkInstanceGenerated(new DateOnly(2026, 4, 1));

        // Then
        recurring.IsDueForGeneration(_today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_MonthlyLastGeneratedDifferentMonth()
    {
        // Given — March 31, evaluating on April 7
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Monthly);
        recurring.MarkInstanceGenerated(new DateOnly(2026, 3, 31));

        // Then
        recurring.IsDueForGeneration(_today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_Paused()
    {
        var recurring = RecurringTask.Create(_title, RecurrencePattern.Daily);
        recurring.Pause();

        recurring.IsDueForGeneration(_today).ShouldBeFalse();
    }
}
