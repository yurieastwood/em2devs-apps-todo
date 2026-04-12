using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for XP anti-inflation mechanics.
/// Maps to: docs/features/progression/experience-points.feature
/// Rule: "The system detects and discourages XP inflation through trivial tasks"
/// </summary>
public sealed class XpAntiInflationTests
{
    private static readonly DateTimeOffset _now = new(2026, 3, 22, 12, 0, 0, TimeSpan.Zero);

    // --- Scenario: Detect burst of trivial task creation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectBurst_When_TrivialCompletionCountExceedsThreshold()
    {
        // Given — 20 trivial completions today (threshold is 5)
        int dailyTrivialCount = 20;

        // When
        bool isBurst = XpCalculator.IsTrivialBurst(dailyTrivialCount);

        // Then
        isBurst.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDetectBurst_When_TrivialCompletionCountIsBelowThreshold()
    {
        // Given — 3 trivial completions today
        int dailyTrivialCount = 3;

        // When
        bool isBurst = XpCalculator.IsTrivialBurst(dailyTrivialCount);

        // Then
        isBurst.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDetectBurst_When_TrivialCompletionCountEqualsThreshold()
    {
        // Given — exactly at threshold (5)
        int dailyTrivialCount = 5;

        // When
        bool isBurst = XpCalculator.IsTrivialBurst(dailyTrivialCount);

        // Then — threshold is inclusive, not a burst
        isBurst.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyReducedRate_When_BurstDetected()
    {
        // Given — 20 trivial tasks completed today (burst scenario)
        int dailyTrivialCount = 20;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, dailyTrivialCount);

        // Then — XP should be significantly reduced compared to first trivial task
        XpBreakdown firstTaskBreakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, 0);

        breakdown.FinalXp.ShouldBeLessThan(firstTaskBreakdown.FinalXp);
        breakdown.FinalXp.ShouldBeGreaterThanOrEqualTo(1);
    }

    // --- Scenario: Repeated trivial tasks earn diminishing returns ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardFullXp_When_TrivialCountWithinThreshold()
    {
        // Given — first 5 trivial tasks get full XP
        for (int count = 0; count <= 4; count++)
        {
            // When
            XpBreakdown breakdown = XpCalculator.Calculate(
                TaskDifficulty.Trivial, null, _now, 0, count);

            // Then — no diminishing returns
            breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyFiftyPercentReduction_When_SixthTrivialTask()
    {
        // Given — 6th trivial task today (count = 5, 0-indexed previous completions)
        int dailyTrivialCount = 5;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, dailyTrivialCount);

        // Then — 50% of base rate
        breakdown.DiminishingReturnsFactor.ShouldBe(0.5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyTwentyFivePercentReduction_When_SeventhTrivialTask()
    {
        // Given — 7th trivial task today
        int dailyTrivialCount = 6;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, dailyTrivialCount);

        // Then — 25% of base rate
        breakdown.DiminishingReturnsFactor.ShouldBe(0.25);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardLessXpForEleventhTask_When_ComparedToFirstTrivialTask()
    {
        // Given — 11th trivial task today (count = 10)
        int dailyTrivialCount = 10;

        // When
        XpBreakdown eleventhTaskBreakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, dailyTrivialCount);
        XpBreakdown firstTaskBreakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, 0);

        // Then
        eleventhTaskBreakdown.FinalXp.ShouldBeLessThan(firstTaskBreakdown.FinalXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowDiminishingReturnsInBreakdown_When_BeyondThreshold()
    {
        // Given — beyond threshold
        int dailyTrivialCount = 7;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, dailyTrivialCount);

        // Then — diminishing factor visible in breakdown
        breakdown.DiminishingReturnsFactor.ShouldBeLessThan(1.0);
        breakdown.DiminishingReturnsFactor.ShouldBeGreaterThan(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotApplyDiminishingReturns_When_DifficultyIsNotTrivial()
    {
        // Given — non-trivial task even with high daily count
        int dailyTrivialCount = 10;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0, dailyTrivialCount);

        // Then — no diminishing returns for non-trivial tasks
        breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NeverAwardZeroXp_When_DiminishingReturnsApplied()
    {
        // Given — extreme case: 50 trivial tasks
        int dailyTrivialCount = 50;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0, dailyTrivialCount);

        // Then — XP floor of 1
        breakdown.FinalXp.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CombineDiminishingReturnsWithOtherModifiers_When_AllApplied()
    {
        // Given — trivial task beyond threshold, early completion, streak active
        int dailyTrivialCount = 5;
        DateTimeOffset deadline = _now.AddDays(2);

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, deadline, _now, 7, dailyTrivialCount);

        // Then — diminishing returns applied alongside other modifiers
        breakdown.DiminishingReturnsFactor.ShouldBe(0.5);
        breakdown.DeadlineModifier.ShouldBe(1.2);
        breakdown.StreakMultiplier.ShouldBeGreaterThan(1.0);
        // FinalXp = round(8 * 1.2 * 1.14 * 0.5) = round(5.472) = 5
        breakdown.FinalXp.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToNoDiminishingReturns_When_DailyCountNotProvided()
    {
        // Given — using the original Calculate overload (no daily count)
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0);

        // Then — default behavior: no diminishing returns
        breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
    }

    // --- Scenario: XP for recurring task completions follows diminishing returns ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardFullXp_When_RecurringTaskCompletedFewTimes()
    {
        // Given — recurring "Easy" task completed 3 times today (threshold is 5)
        int dailyRecurringCount = 3;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Easy, null, _now, 0,
            dailyTrivialCompletionCount: 0,
            dailyRecurringCompletionCount: dailyRecurringCount);

        // Then — no diminishing returns yet
        breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyDiminishingReturns_When_RecurringTaskExceedsThreshold()
    {
        // Given — recurring "Easy" task completed 6 times today (threshold is 5)
        int dailyRecurringCount = 5;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Easy, null, _now, 0,
            dailyTrivialCompletionCount: 0,
            dailyRecurringCompletionCount: dailyRecurringCount);

        // Then — 50% diminishing returns
        breakdown.DiminishingReturnsFactor.ShouldBe(0.5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyRecurringDiminishingReturns_When_DifficultyIsNormal()
    {
        // Given — recurring "Normal" task completed 6 times today
        int dailyRecurringCount = 6;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0,
            dailyTrivialCompletionCount: 0,
            dailyRecurringCompletionCount: dailyRecurringCount);

        // Then — diminishing returns apply regardless of difficulty for recurring tasks
        breakdown.DiminishingReturnsFactor.ShouldBe(0.25);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MultiplyDiminishingFactors_When_BothTrivialAndRecurring()
    {
        // Given — trivial recurring task beyond both thresholds
        // trivial count=7 → factor = 0.5^((7-5)+1) = 0.5^3 = 0.125
        // recurring count=6 → factor = 0.5^((6-5)+1) = 0.5^2 = 0.25
        // combined = 0.125 * 0.25 = 0.03125
        int dailyTrivialCount = 7;
        int dailyRecurringCount = 6;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0,
            dailyTrivialCompletionCount: dailyTrivialCount,
            dailyRecurringCompletionCount: dailyRecurringCount);

        // Then — factors are multiplied together
        breakdown.DiminishingReturnsFactor.ShouldBe(0.03125, 0.0001);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NeverAwardZeroXp_When_RecurringDiminishingReturnsApplied()
    {
        // Given — extreme case: 50 recurring completions
        int dailyRecurringCount = 50;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Easy, null, _now, 0,
            dailyTrivialCompletionCount: 0,
            dailyRecurringCompletionCount: dailyRecurringCount);

        // Then — XP floor of 1
        breakdown.FinalXp.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotApplyRecurringDiminishing_When_RecurringCountNotProvided()
    {
        // Given — no recurring count provided (default 0)
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0);

        // Then — no diminishing returns
        breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFactorOfOne_When_RecurringCountIsZeroForNonTrivialTask()
    {
        // Given — non-trivial task with no recurring or trivial counts
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0,
            dailyTrivialCompletionCount: 0,
            dailyRecurringCompletionCount: 0);

        // Then — factor should be exactly 1.0 (trivial=1.0 * recurring=1.0)
        breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
        breakdown.FinalXp.ShouldBe(breakdown.BaseXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFactorOfOne_When_TrivialCountAtExactThresholdMinusOne()
    {
        // Given — trivial tasks at count 4 (just below threshold of 5)
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Trivial, null, _now, 0,
            dailyTrivialCompletionCount: 4,
            dailyRecurringCompletionCount: 0);

        // Then — no diminishing returns applied
        breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFactorOfOne_When_RecurringCountAtExactThresholdMinusOne()
    {
        // Given — recurring tasks at count 4 (just below threshold of 5)
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0,
            dailyTrivialCompletionCount: 0,
            dailyRecurringCompletionCount: 4);

        // Then — no diminishing returns applied
        breakdown.DiminishingReturnsFactor.ShouldBe(1.0);
    }
}
