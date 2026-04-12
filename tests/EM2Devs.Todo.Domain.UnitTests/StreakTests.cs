using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Streak value object.
/// Maps to: docs/features/progression/streaks.feature
/// Rule: "Streaks track consecutive days of completing at least one task"
/// Rule: "Grace days protect streaks from occasional missed days"
/// </summary>
public sealed class StreakTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);
    private static readonly DateOnly _yesterday = _today.AddDays(-1);
    private static readonly DateOnly _twoDaysAgo = _today.AddDays(-2);

    // --- Rule: Streak tracking ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartWithZeroStreak_When_NewStreakCreated()
    {
        // Given / When
        var streak = Streak.NewStreak();

        // Then
        streak.CurrentDays.ShouldBe(0);
        streak.GraceDaysAvailable.ShouldBe(0);
        streak.LastActiveDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementStreak_When_CompletingFirstTaskOfDay()
    {
        // Given — streak of 5 days, last active yesterday
        var streak = new Streak(5, _yesterday, 0);

        // When — complete a task today
        var result = streak.RecordCompletion(_today);

        // Then — streak increments to 6
        result.CurrentDays.ShouldBe(6);
        result.LastActiveDate.ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotIncrementStreak_When_AlreadyCompleted_today()
    {
        // Given — streak of 10 days, already active today
        var streak = new Streak(10, _today, 0);

        // When — complete another task today
        var result = streak.RecordCompletion(_today);

        // Then — streak remains at 10
        result.CurrentDays.ShouldBe(10);
        result.LastActiveDate.ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartStreakAtOne_When_FirstEverCompletion()
    {
        // Given — brand new streak
        var streak = Streak.NewStreak();

        // When — complete first task ever
        var result = streak.RecordCompletion(_today);

        // Then — streak starts at 1
        result.CurrentDays.ShouldBe(1);
        result.LastActiveDate.ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetAndStartNewStreak_When_CompletingAfterMissedDay()
    {
        // Given — streak of 5, last active 2 days ago (missed yesterday, no grace days)
        var streak = new Streak(5, _twoDaysAgo, 0);

        // When — complete a task today
        var result = streak.RecordCompletion(_today);

        // Then — streak resets to 1 (new streak started)
        result.CurrentDays.ShouldBe(1);
        result.LastActiveDate.ShouldBe(_today);
    }

    // --- Rule: Grace days ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveStreak_When_GraceDayAvailableOnMissedDay()
    {
        // Given — streak of 15, last active yesterday, 1 grace day
        var streak = new Streak(15, _yesterday, 1);

        // When — day ends without completion (today)
        var result = streak.ProcessDayEnd(_today);

        // Then — streak preserved, grace day consumed
        result.CurrentDays.ShouldBe(15);
        result.GraceDaysAvailable.ShouldBe(0);
        result.LastActiveDate.ShouldBe(_yesterday);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotConsumeGraceDay_When_TaskCompleted_today()
    {
        // Given — streak of 15, already active today, 2 grace days
        var streak = new Streak(16, _today, 2);

        // When — day ends (already completed)
        var result = streak.ProcessDayEnd(_today);

        // Then — grace days unchanged
        result.CurrentDays.ShouldBe(16);
        result.GraceDaysAvailable.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_NoGraceDaysOnMissedDay()
    {
        // Given — streak of 20, last active yesterday, 0 grace days
        var streak = new Streak(20, _yesterday, 0);

        // When — day ends without completion (today is day after yesterday, missed today)
        var endOf_today = _today.AddDays(1);
        var result = streak.ProcessDayEnd(endOf_today);

        // Then — streak resets to 0
        result.CurrentDays.ShouldBe(0);
        result.GraceDaysAvailable.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddGraceDay_When_EarningOne()
    {
        // Given — 0 grace days
        var streak = new Streak(5, _today, 0);

        // When
        var result = streak.AddGraceDay();

        // Then
        result.GraceDaysAvailable.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapGraceDays_When_AtMaximum()
    {
        // Given — already at max grace days (3)
        var streak = new Streak(5, _today, Streak.MaxGraceDays);

        // When
        var result = streak.AddGraceDay();

        // Then — stays at max
        result.GraceDaysAvailable.ShouldBe(Streak.MaxGraceDays);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_ProcessDayEndCalledOnActiveDay()
    {
        // Given — already active today
        var streak = new Streak(10, _today, 1);

        // When — day ends but we already completed today
        var result = streak.ProcessDayEnd(_today);

        // Then — unchanged
        result.CurrentDays.ShouldBe(10);
        result.GraceDaysAvailable.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_ProcessDayEndCalledWithZeroStreak()
    {
        // Given — no active streak
        var streak = Streak.NewStreak();

        // When — day ends
        var result = streak.ProcessDayEnd(_today);

        // Then — still zero
        result.CurrentDays.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_ProcessDayEndCalledWithZeroStreakAndPastDate()
    {
        // Given — streak reset but has a past active date and grace days
        var streak = new Streak(0, _yesterday, 1);

        // When — day ends
        var result = streak.ProcessDayEnd(_today);

        // Then — no change, no grace day consumed (nothing to protect)
        result.CurrentDays.ShouldBe(0);
        result.GraceDaysAvailable.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_ProcessDayEndCalledWithNullLastActiveDate()
    {
        // Given — streak with days but no last active date (edge case)
        var streak = new Streak(5, null, 0);

        // When — day ends
        var result = streak.ProcessDayEnd(_today);

        // Then — unchanged (no date to compare against)
        result.CurrentDays.ShouldBe(5);
    }

    // --- Validation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CurrentDaysIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new Streak(-1, _today, 0));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GraceDaysIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new Streak(0, null, -1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_GraceDaysExceedMax()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Streak(0, null, Streak.MaxGraceDays + 1));
        ex.Message.ShouldContain("cannot exceed");
    }
}
