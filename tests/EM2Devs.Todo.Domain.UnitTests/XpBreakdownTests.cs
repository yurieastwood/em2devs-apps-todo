using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for XP calculation with deadline and streak modifiers.
/// Maps to: docs/features/progression/experience-points.feature
/// </summary>
public sealed class XpBreakdownTests
{
    private static readonly DateTimeOffset _now = new(2026, 3, 22, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyEarlyBonus_When_CompletedBeforeDeadline()
    {
        // Given — task due in 3 days, completed 2 days early
        DateTimeOffset deadline = _now.AddDays(3);
        DateTimeOffset completedAt = _now;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Normal, deadline, completedAt, 0);

        // Then
        breakdown.DeadlineModifier.ShouldBe(1.2);
        breakdown.FinalXp.ShouldBeGreaterThan(breakdown.BaseXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyLatePenalty_When_CompletedAfterDeadline()
    {
        // Given — task due yesterday, completed today
        DateTimeOffset deadline = _now.AddDays(-1);
        DateTimeOffset completedAt = _now;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Normal, deadline, completedAt, 0);

        // Then
        breakdown.DeadlineModifier.ShouldBe(0.8);
        breakdown.FinalXp.ShouldBeLessThan(breakdown.BaseXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NeverReturnNegativeXp_When_TaskIsVeryOverdue()
    {
        // Given — 30 days overdue
        DateTimeOffset deadline = _now.AddDays(-30);
        DateTimeOffset completedAt = _now;

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Trivial, deadline, completedAt, 0);

        // Then
        breakdown.FinalXp.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyStreakMultiplier_When_UserHasActiveStreak()
    {
        // Given — 7-day streak
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Normal, null, _now, 7);

        // Then — 1.0 + (7 * 0.02) = 1.14
        breakdown.StreakMultiplier.ShouldBe(1.14, 0.001);
        breakdown.FinalXp.ShouldBeGreaterThan(breakdown.BaseXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapStreakMultiplier_When_StreakExceedsMaxDays()
    {
        // Given — 50-day streak (capped at 30)
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Normal, null, _now, 50);

        // Then — 1.0 + (30 * 0.02) = 1.60
        breakdown.StreakMultiplier.ShouldBe(1.6);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseNormalDifficulty_When_NoDifficultyIsSet()
    {
        // Given — null difficulty
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(null, null, _now, 0);

        // Then — should use Normal base XP (30)
        breakdown.BaseXp.ShouldBe(ExperiencePoints.BaseForDifficulty(TaskDifficulty.Normal).Value);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTransparentBreakdown_When_AllModifiersApplied()
    {
        // Given — Hard task, completed early, 10-day streak
        DateTimeOffset deadline = _now.AddDays(2);

        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Hard, deadline, _now, 10);

        // Then
        breakdown.BaseXp.ShouldBe(60);
        breakdown.DeadlineModifier.ShouldBe(1.2);
        breakdown.StreakMultiplier.ShouldBe(1.2);  // 1.0 + (10 * 0.02)
        breakdown.FinalXp.ShouldBe(86);  // round(60 * 1.2 * 1.2) = round(86.4) = 86
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnExperiencePoints_When_ConvertingBreakdown()
    {
        // Given
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Normal, null, _now, 0);

        // When
        ExperiencePoints xp = breakdown.ToExperiencePoints();

        // Then
        xp.Value.ShouldBe(breakdown.FinalXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseNoModifier_When_NoDeadlineSet()
    {
        // Given — no deadline
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Normal, null, _now, 0);

        // Then
        breakdown.DeadlineModifier.ShouldBe(1.0);
        breakdown.FinalXp.ShouldBe(breakdown.BaseXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseNoStreakBonus_When_StreakIsZero()
    {
        // Given — no streak
        // When
        XpBreakdown breakdown = XpCalculator.Calculate(TaskDifficulty.Normal, null, _now, 0);

        // Then
        breakdown.StreakMultiplier.ShouldBe(1.0);
    }
}
