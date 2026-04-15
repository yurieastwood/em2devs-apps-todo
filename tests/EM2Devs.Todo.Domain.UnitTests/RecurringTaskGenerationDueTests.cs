using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Tests for RecurringTask.IsDueForGeneration(lastScheduledDate, today) — a pure function
/// that decides whether a new instance should be generated given the last scheduled date
/// (derived from the instance table by the caller) and today's date.
///
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
        // Given — no instances yet, so lastScheduledDate is null
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily);

        // Then
        recurring.IsDueForGeneration(lastScheduledDate: null, today: _today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_DailyLastScheduledIsToday()
    {
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily);

        recurring.IsDueForGeneration(lastScheduledDate: _today, today: _today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_DailyLastScheduledIsYesterday()
    {
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily);

        recurring.IsDueForGeneration(lastScheduledDate: _today.AddDays(-1), today: _today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_WeeklyLastScheduled6DaysAgo()
    {
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Weekly);

        recurring.IsDueForGeneration(lastScheduledDate: _today.AddDays(-6), today: _today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_WeeklyLastScheduled7DaysAgo()
    {
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Weekly);

        recurring.IsDueForGeneration(lastScheduledDate: _today.AddDays(-7), today: _today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_MonthlyLastScheduledSameMonth()
    {
        // Given — both dates in April 2026
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Monthly);

        // Then
        recurring
            .IsDueForGeneration(lastScheduledDate: new DateOnly(2026, 4, 1), today: _today)
            .ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_MonthlyLastScheduledDifferentMonth()
    {
        // Given — March 31, evaluating on April 7
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Monthly);

        // Then
        recurring
            .IsDueForGeneration(lastScheduledDate: new DateOnly(2026, 3, 31), today: _today)
            .ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_Paused()
    {
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily);
        recurring.Pause();

        // Even with no instances yet, a paused task is not due.
        recurring.IsDueForGeneration(lastScheduledDate: null, today: _today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_PatternIsUnrecognisedEnumValue()
    {
        // Given — cast an out-of-range int to RecurrencePattern to exercise the
        // switch's default arm. Prevents a silently-added enum value from quietly
        // generating instances without an explicit schedule decision.
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, (RecurrencePattern)999);

        // Then — default arm returns false, even with a prior instance present
        recurring
            .IsDueForGeneration(lastScheduledDate: _today.AddDays(-30), today: _today)
            .ShouldBeFalse();
    }

    // --- Scenario: Create a recurring task with an end date ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeDue_When_TodayIsAfterEndDate()
    {
        // Given — end date is in the future from now, but we test with a "today" past it
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily, endDate: endDate);

        // Then — evaluating at a date past end date → not due
        recurring.IsDueForGeneration(lastScheduledDate: null, today: endDate.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_TodayEqualsEndDate()
    {
        // Given
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily, endDate: endDate);

        // Then — today equals end date → still due
        recurring.IsDueForGeneration(lastScheduledDate: endDate.AddDays(-1), today: endDate).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_TodayIsBeforeEndDate()
    {
        // Given
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily, endDate: endDate);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Then
        recurring.IsDueForGeneration(lastScheduledDate: today.AddDays(-1), today: today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeDue_When_NoEndDateSet()
    {
        // Given — no end date, should behave as before
        var recurring = RecurringTask.Create(TestData.TestUserId, _title, RecurrencePattern.Daily);

        // Then
        recurring.IsDueForGeneration(lastScheduledDate: _today.AddDays(-1), today: _today).ShouldBeTrue();
    }
}
