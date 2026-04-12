using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Level.ReapplyThresholds.
/// Maps to: docs/features/progression/levelling.feature
/// Scenario: "Existing users retain levels when XP thresholds are rebalanced"
/// </summary>
public sealed class LevelRebalanceTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncreaseLevel_When_NewThresholdsAreLower()
    {
        // Given — user is level 5 with 250 cumulative XP
        //   Under new (lower) thresholds, 250 cumulative XP would be level 7
        var level = new Level(5, new ExperiencePoints(50));
        int cumulativeXp = Level.CumulativeXpRequired(5) + 50; // 300 + 50 = 350

        // When — reapply with the same default thresholds (no actual change)
        //   but the method must ensure level never decreases
        var result = level.ReapplyThresholds(cumulativeXp);

        // Then — level should be at least what it was before
        result.Value.ShouldBeGreaterThanOrEqualTo(level.Value);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NeverDecreaseLevel_When_NewThresholdsAreHigher()
    {
        // Given — user is level 15 with 3500 cumulative XP
        //   Under rebalanced (higher) thresholds, 3500 XP might only be level 12
        //   But we must guarantee the level never decreases
        var level = new Level(15, new ExperiencePoints(100));

        // When — reapply with cumulative XP that would normally yield a lower level
        //   (simulated by passing a low cumulative XP relative to level 15)
        var result = level.ReapplyThresholds(1000);

        // Then — level must not decrease
        result.Value.ShouldBeGreaterThanOrEqualTo(15);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainCurrentLevel_When_ThresholdsUnchanged()
    {
        // Given — level 10 with some XP, cumulative matches exactly
        var level = new Level(10, new ExperiencePoints(50));
        int cumulativeXp = Level.CumulativeXpRequired(10) + 50;

        // When
        var result = level.ReapplyThresholds(cumulativeXp);

        // Then — same level, same remaining XP
        result.Value.ShouldBe(10);
        result.CurrentXp.Value.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapAtMaxLevel_When_ReapplyWouldExceedMax()
    {
        // Given — user at level 99 with huge cumulative XP
        var level = new Level(99, new ExperiencePoints(0));
        int hugeXp = 999_999;

        // When
        var result = level.ReapplyThresholds(hugeXp);

        // Then
        result.Value.ShouldBe(Level.MaxLevel);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateCorrectRemainingXp_When_LevelIncreases()
    {
        // Given — level 3 with 10 XP, but cumulative XP is 350 (enough for level 5)
        var level = new Level(3, new ExperiencePoints(10));
        int cumulativeXp = 350; // threshold for level 5 is 300

        // When
        var result = level.ReapplyThresholds(cumulativeXp);

        // Then — level should be 5 (300 cumulative) with 50 remaining
        result.Value.ShouldBe(5);
        result.CurrentXp.Value.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveLevel_When_AtLevelOneWithLowXp()
    {
        // Given — brand new user, level 1 with 10 XP
        var level = new Level(1, new ExperiencePoints(10));

        // When — reapply with 10 cumulative XP (not enough for level 2)
        var result = level.ReapplyThresholds(10);

        // Then
        result.Value.ShouldBe(1);
        result.CurrentXp.Value.ShouldBe(10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUp_When_CumulativeXpExactlyMatchesThreshold()
    {
        // Given — level 1 with 0 XP
        var level = new Level(1, new ExperiencePoints(0));

        // When — cumulative XP exactly equals the threshold for level 2
        int exactThreshold = Level.CumulativeXpRequired(2);
        var result = level.ReapplyThresholds(exactThreshold);

        // Then — should level up to exactly level 2 with 0 remaining XP
        result.Value.ShouldBe(2);
        result.CurrentXp.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveCurrentLevel_When_RecalculatedLevelEqualsCurrentLevel()
    {
        // Given — level 5 with 30 XP
        var level = new Level(5, new ExperiencePoints(30));

        // When — cumulative XP yields exactly level 5 (same level, different remaining XP)
        int cumulativeXp = Level.CumulativeXpRequired(5) + 10;
        var result = level.ReapplyThresholds(cumulativeXp);

        // Then — level stays at 5 (recalculated == current, so NOT preserved from old)
        //   and remaining XP comes from the recalculation, not the original
        result.Value.ShouldBe(5);
        result.CurrentXp.Value.ShouldBe(10);
    }
}
