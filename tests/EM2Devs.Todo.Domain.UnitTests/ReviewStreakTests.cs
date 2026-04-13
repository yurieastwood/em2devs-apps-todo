using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ReviewStreak value object.
/// Maps to: docs/features/reflection/weekly-review.feature
/// Rule: "Completing weekly reviews earns XP and maintains a review streak"
/// </summary>
public sealed class ReviewStreakTests
{
    private static readonly DateOnly _weekStart = new(2026, 3, 9); // Monday
    private static readonly DateOnly _nextWeek = _weekStart.AddDays(7);
    private static readonly DateOnly _twoWeeksLater = _weekStart.AddDays(14);
    private static readonly DateOnly _threeWeeksLater = _weekStart.AddDays(21);

    // ── Scenario: Review streak builds over weeks ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartWithZero_When_NewStreakCreated()
    {
        ReviewStreak streak = ReviewStreak.NewStreak();

        streak.ConsecutiveWeeks.ShouldBe(0);
        streak.LastReviewWeek.ShouldBeNull();
        streak.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeOneWeek_When_FirstReviewCompleted()
    {
        ReviewStreak streak = ReviewStreak.NewStreak();

        ReviewStreak result = streak.RecordCompletion(_weekStart);

        result.ConsecutiveWeeks.ShouldBe(1);
        result.LastReviewWeek.ShouldBe(_weekStart);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementTo12_When_11ConsecutiveWeeksAndCompletingCurrent()
    {
        ReviewStreak streak = new(11, _weekStart);

        ReviewStreak result = streak.RecordCompletion(_nextWeek);

        result.ConsecutiveWeeks.ShouldBe(12);
        result.LastReviewWeek.ShouldBe(_nextWeek);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotChange_When_SameWeekCompleted()
    {
        ReviewStreak streak = new(5, _weekStart);

        ReviewStreak result = streak.RecordCompletion(_weekStart);

        result.ConsecutiveWeeks.ShouldBe(5);
    }

    // ── Scenario: Missed review does not break streak harshly ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PauseStreak_When_WeekMissed()
    {
        ReviewStreak streak = new(8, _weekStart);

        ReviewStreak paused = streak.MissWeek();

        paused.IsPaused.ShouldBeTrue();
        paused.ConsecutiveWeeks.ShouldBe(8);
        paused.LastReviewWeek.ShouldBe(_weekStart);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueFromPaused_When_NextWeekCompleted()
    {
        ReviewStreak streak = new(8, _weekStart, isPaused: true);

        ReviewStreak result = streak.RecordCompletion(_twoWeeksLater);

        result.ConsecutiveWeeks.ShouldBe(9);
        result.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetToZero_When_PausedAndMissedAgain()
    {
        ReviewStreak streak = new(8, _weekStart, isPaused: true);

        ReviewStreak result = streak.MissWeek();

        result.ConsecutiveWeeks.ShouldBe(0);
        result.IsPaused.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GracePeriodBeOneWeek_When_Checked()
    {
        ReviewStreak.GracePeriodWeeks.ShouldBe(1);
    }

    // ── Scenario: Complete two missed weeks during the grace period ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountBothMissedAndCurrent_When_CatchingUpDuringGrace()
    {
        // Given — streak of 5, paused
        ReviewStreak streak = new(5, _weekStart, isPaused: true);

        // When — complete missed week
        ReviewStreak afterMissed = streak.RecordCompletion(_nextWeek);

        // And — complete current week
        ReviewStreak afterCurrent = afterMissed.RecordCompletion(_twoWeeksLater);

        // Then — streak is 7 (5 + 2)
        afterCurrent.ConsecutiveWeeks.ShouldBe(7);
    }

    // ── Reset scenarios ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetToOne_When_TooManyWeeksMissed()
    {
        ReviewStreak streak = new(5, _weekStart);

        ReviewStreak result = streak.RecordCompletion(_threeWeeksLater);

        result.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSelf_When_MissingWithZeroStreak()
    {
        ReviewStreak streak = new(0, null);

        ReviewStreak result = streak.MissWeek();

        result.ShouldBe(streak);
    }

    // ── Week start calculation ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMonday_When_DateIsMonday()
    {
        DateOnly monday = new(2026, 3, 9);
        ReviewStreak.GetWeekStart(monday).ShouldBe(monday);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnPreviousMonday_When_DateIsSunday()
    {
        DateOnly sunday = new(2026, 3, 15);
        ReviewStreak.GetWeekStart(sunday).ShouldBe(new DateOnly(2026, 3, 9));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnPreviousMonday_When_DateIsWednesday()
    {
        DateOnly wednesday = new(2026, 3, 11);
        ReviewStreak.GetWeekStart(wednesday).ShouldBe(new DateOnly(2026, 3, 9));
    }

    // ── Validation ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NegativeConsecutiveWeeks()
    {
        Should.Throw<DomainException>(() => new ReviewStreak(-1, _weekStart));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveValueEquality_When_SameProperties()
    {
        ReviewStreak a = new(5, _weekStart);
        ReviewStreak b = new(5, _weekStart);

        a.ShouldBe(b);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnpauseStreak_When_SameWeekCompletedWhilePaused()
    {
        ReviewStreak streak = new(5, _weekStart, isPaused: true);

        ReviewStreak result = streak.RecordCompletion(_weekStart);

        result.IsPaused.ShouldBeFalse();
        result.ConsecutiveWeeks.ShouldBe(5);
    }

    // ── Mutation-killing: DomainException message ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NegativeConsecutiveWeeks()
    {
        DomainException ex = Should.Throw<DomainException>(() => new ReviewStreak(-1, _weekStart));
        ex.Message.ShouldContain("Consecutive weeks cannot be negative");
    }

    // ── Mutation-killing: IsPaused ternary in same-week completion ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSelf_When_SameWeekCompletedAndNotPaused()
    {
        ReviewStreak streak = new(5, _weekStart, isPaused: false);

        ReviewStreak result = streak.RecordCompletion(_weekStart);

        // Should return this (same instance) when not paused
        result.ShouldBe(streak);
        result.IsPaused.ShouldBeFalse();
    }

    // ── Mutation-killing: weeksDifference == 2 boundary for non-paused ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_TwoWeeksGapAndNotPaused()
    {
        // 2 weeks gap but NOT paused — should reset
        ReviewStreak streak = new(5, _weekStart, isPaused: false);

        ReviewStreak result = streak.RecordCompletion(_twoWeeksLater);

        result.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetStreak_When_ThreeWeeksGapAndPaused()
    {
        // 3 weeks gap and paused — beyond grace, should reset
        ReviewStreak streak = new(5, _weekStart, isPaused: true);

        ReviewStreak result = streak.RecordCompletion(_threeWeeksLater);

        result.ConsecutiveWeeks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueStreak_When_ExactlyTwoWeeksAndPaused()
    {
        // Exactly 2 weeks gap AND paused — within grace
        ReviewStreak streak = new(5, _weekStart, isPaused: true);

        ReviewStreak result = streak.RecordCompletion(_twoWeeksLater);

        result.ConsecutiveWeeks.ShouldBe(6);
    }

    // ── Mutation-killing: consecutive week increments by exactly 1 ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementByExactlyOne_When_ConsecutiveWeek()
    {
        ReviewStreak streak = new(3, _weekStart);

        ReviewStreak result = streak.RecordCompletion(_nextWeek);

        result.ConsecutiveWeeks.ShouldBe(4);
        result.LastReviewWeek.ShouldBe(_nextWeek);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementByExactlyOne_When_GracePeriodCompletion()
    {
        ReviewStreak streak = new(3, _weekStart, isPaused: true);

        ReviewStreak result = streak.RecordCompletion(_twoWeeksLater);

        result.ConsecutiveWeeks.ShouldBe(4);
    }

    // ── Mutation-killing: IsPaused conditional returns this when not paused ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameReference_When_SameWeekCompletedAndNotPaused()
    {
        ReviewStreak streak = new(5, _weekStart, isPaused: false);

        ReviewStreak result = streak.RecordCompletion(_weekStart);

        ReferenceEquals(result, streak).ShouldBeTrue();
    }

    // ── Mutation-killing: MissWeek resets to exactly 0 ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetToExactlyZero_When_PausedAndMissedAgain()
    {
        ReviewStreak streak = new(10, _weekStart, isPaused: true);

        ReviewStreak result = streak.MissWeek();

        result.ConsecutiveWeeks.ShouldBe(0);
        result.IsPaused.ShouldBeFalse();
        result.LastReviewWeek.ShouldBe(_weekStart);
    }
}
