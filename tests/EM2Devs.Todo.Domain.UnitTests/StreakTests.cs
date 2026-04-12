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
    private static readonly int[] _expectedMilestoneThresholds = [7, 14, 30, 60, 100, 365];
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

    // --- Rule: Streak milestones ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMilestone_When_StreakReachesMilestoneThreshold()
    {
        // Given — streak of 6 days, last active yesterday
        var streak = new Streak(6, _yesterday, 0);

        // When — complete a task and streak reaches 7
        var result = streak.RecordCompletion(_today);

        // Then — milestone detected
        result.CurrentDays.ShouldBe(7);
        var milestone = result.CheckMilestone();
        milestone.ShouldNotBeNull();
        milestone.Days.ShouldBe(7);
        milestone.Label.ShouldBe("One Week");
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(6, 7, "One Week")]
    [InlineData(13, 14, "Two Weeks")]
    [InlineData(29, 30, "One Month")]
    [InlineData(59, 60, "Two Months")]
    [InlineData(99, 100, "The Century")]
    [InlineData(364, 365, "The Full Year")]
    public void Should_CelebrateMilestone_When_StreakReachesKeyThreshold(
        int previousDays, int expectedDays, string expectedLabel)
    {
        // Given — streak at previous_days, last active yesterday
        var yesterday = _today.AddDays(-1);
        var streak = new Streak(previousDays, yesterday, 0);

        // When — complete a task today
        var result = streak.RecordCompletion(_today);

        // Then — milestone celebration triggered
        result.CurrentDays.ShouldBe(expectedDays);
        var milestone = result.CheckMilestone();
        milestone.ShouldNotBeNull();
        milestone.Label.ShouldBe(expectedLabel);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_StreakIsNotAtMilestone()
    {
        // Given — streak of 4 days, last active yesterday
        var streak = new Streak(4, _yesterday, 0);

        // When — complete a task and streak reaches 5 (not a milestone)
        var result = streak.RecordCompletion(_today);

        // Then — no milestone
        result.CurrentDays.ShouldBe(5);
        result.CheckMilestone().ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ListAllMilestoneThresholds_When_ThresholdsQueried()
    {
        // Given / When
        var thresholds = StreakMilestone.Thresholds;

        // Then
        thresholds.ShouldBe(_expectedMilestoneThresholds);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNullMilestone_When_DaysNotInThresholds()
    {
        // Given / When / Then
        StreakMilestone.ForDays(0).ShouldBeNull();
        StreakMilestone.ForDays(1).ShouldBeNull();
        StreakMilestone.ForDays(50).ShouldBeNull();
        StreakMilestone.ForDays(200).ShouldBeNull();
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

    // --- Rule: Users can manually freeze their streak ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FreezeStreak_When_ActivatingStreakFreeze()
    {
        // Given — streak of 30 days
        var streak = new Streak(30, _yesterday, 0);

        // When — activate a streak freeze for 5 days
        var frozenAt = _today;
        var result = streak.Freeze(frozenAt, 5);

        // Then — streak is frozen at 30 days
        result.IsFrozen.ShouldBeTrue();
        result.CurrentDays.ShouldBe(30);
        result.ActiveFreeze.ShouldNotBeNull();
        result.ActiveFreeze!.FrozenAt.ShouldBe(frozenAt);
        result.ActiveFreeze.Duration.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FreezeDurationExceedsMaximum()
    {
        // Given — any streak
        var streak = new Streak(10, _yesterday, 0);

        // When / Then — attempting to freeze for 15 days (max is 7)
        var ex = Should.Throw<DomainException>(() => streak.Freeze(_today, 15));
        ex.Message.ShouldContain("maximum freeze duration is 7 days");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowMaxFreezeDuration_When_FreezingForExactly7Days()
    {
        // Given
        var streak = new Streak(10, _yesterday, 0);

        // When — freeze for exactly 7 days (the max)
        var result = streak.Freeze(_today, StreakFreeze.MaxFreezeDuration);

        // Then — freeze accepted
        result.IsFrozen.ShouldBeTrue();
        result.ActiveFreeze!.Duration.ShouldBe(7);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FreezeDurationIsZeroOrNegative()
    {
        // Given
        var streak = new Streak(10, _yesterday, 0);

        // When / Then
        var ex = Should.Throw<DomainException>(() => streak.Freeze(_today, 0));
        ex.Message.ShouldContain("at least 1 day");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBreakStreak_When_DayEndsDuringFreeze()
    {
        // Given — streak frozen at 30 days for 5 days
        var frozenAt = _today;
        var streak = new Streak(30, _yesterday, 0).Freeze(frozenAt, 5);

        // When — day ends during freeze (day 2 of freeze)
        var dayDuringFreeze = frozenAt.AddDays(1);
        var result = streak.ProcessDayEnd(dayDuringFreeze);

        // Then — streak preserved, still frozen
        result.CurrentDays.ShouldBe(30);
        result.IsFrozen.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResumeStreak_When_FreezeExpires()
    {
        // Given — streak frozen at 30 days for 5 days
        var frozenAt = _today;
        var streak = new Streak(30, _yesterday, 0).Freeze(frozenAt, 5);

        // When — freeze period ends (process day end after freeze expires)
        var afterFreeze = frozenAt.AddDays(5);
        var result = streak.ProcessDayEnd(afterFreeze);

        // Then — freeze expired, streak resumed (not broken)
        result.IsFrozen.ShouldBeFalse();
        result.CurrentDays.ShouldBe(30);
        result.ActiveFreeze.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContinueFromPreviousStreak_When_CompletingTaskAfterFreezeEnds()
    {
        // Given — streak was frozen at 30 days for 5 days, freeze has ended
        var frozenAt = _today;
        var streak = new Streak(30, _yesterday, 0).Freeze(frozenAt, 5);
        var afterFreeze = frozenAt.AddDays(5);
        var unfrozenStreak = streak.ProcessDayEnd(afterFreeze);

        // When — complete a task the next day after freeze ends
        var nextDay = afterFreeze.AddDays(1);
        var result = unfrozenStreak.RecordCompletion(nextDay);

        // Then — streak continues from 31
        result.CurrentDays.ShouldBe(31);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepFreezeActive_When_CompletingTaskDuringFreeze()
    {
        // Given — streak frozen at 30 days for 5 days, on day 2
        var frozenAt = _today;
        var streak = new Streak(30, _yesterday, 0).Freeze(frozenAt, 5);

        // When — complete a task during the freeze
        var day2OfFreeze = frozenAt.AddDays(1);
        var result = streak.RecordCompletion(day2OfFreeze);

        // Then — freeze remains active, streak stays at 30
        result.IsFrozen.ShouldBeTrue();
        result.CurrentDays.ShouldBe(30);
        result.LastActiveDate.ShouldBe(day2OfFreeze);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AutoExpireFreeze_When_ProcessDayEndAfterMaxDuration()
    {
        // Given — streak frozen at 30 days for 7 days (max)
        var frozenAt = _today;
        var streak = new Streak(30, _yesterday, 0).Freeze(frozenAt, 7);

        // When — process day end after 7 days
        var afterMax = frozenAt.AddDays(7);
        var result = streak.ProcessDayEnd(afterMax);

        // Then — freeze auto-expired
        result.IsFrozen.ShouldBeFalse();
        result.CurrentDays.ShouldBe(30);
        result.ActiveFreeze.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ManuallyUnfreeze_When_UnfreezeCalledDuringFreeze()
    {
        // Given — streak frozen
        var streak = new Streak(30, _yesterday, 0).Freeze(_today, 5);

        // When — manually unfreeze
        var result = streak.Unfreeze(_today);

        // Then — unfrozen
        result.IsFrozen.ShouldBeFalse();
        result.CurrentDays.ShouldBe(30);
        result.ActiveFreeze.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_UnfreezeCalledOnUnfrozenStreak()
    {
        // Given — not frozen
        var streak = new Streak(10, _yesterday, 0);

        // When
        var result = streak.Unfreeze(_today);

        // Then — unchanged
        result.ShouldBe(streak);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FreezingAlreadyFrozenStreak()
    {
        // Given — already frozen
        var streak = new Streak(30, _yesterday, 0).Freeze(_today, 5);

        // When / Then
        var ex = Should.Throw<DomainException>(() => streak.Freeze(_today, 3));
        ex.Message.ShouldContain("already frozen");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotConsumeGraceDay_When_FrozenAndDayMissed()
    {
        // Given — streak frozen with grace days available
        var streak = new Streak(30, _yesterday, 2).Freeze(_today, 5);

        // When — day ends during freeze
        var result = streak.ProcessDayEnd(_today.AddDays(1));

        // Then — grace days not consumed
        result.GraceDaysAvailable.ShouldBe(2);
        result.CurrentDays.ShouldBe(30);
    }
}
