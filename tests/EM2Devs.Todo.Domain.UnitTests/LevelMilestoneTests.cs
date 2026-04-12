using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for LevelMilestone.
/// Maps to: docs/features/progression/levelling.feature
/// Scenario: "Level milestones are celebrated"
/// </summary>
public sealed class LevelMilestoneTests
{
    private static readonly int[] _expectedThresholds = [10, 25, 50, 100];
    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_ReturnMilestone_When_LevelIsMilestoneLevel(int level)
    {
        // Given / When
        var milestone = LevelMilestone.ForLevel(level);

        // Then
        milestone.ShouldNotBeNull();
        milestone.Level.ShouldBe(level);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(11)]
    [InlineData(24)]
    [InlineData(26)]
    [InlineData(49)]
    [InlineData(51)]
    [InlineData(99)]
    public void Should_ReturnNull_When_LevelIsNotMilestoneLevel(int level)
    {
        // Given / When
        var milestone = LevelMilestone.ForLevel(level);

        // Then
        milestone.ShouldBeNull();
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(10, "Double Digits")]
    [InlineData(25, "Quarter Century")]
    [InlineData(50, "Half Century")]
    [InlineData(100, "The Centurion")]
    public void Should_HaveCorrectLabel_When_MilestoneReached(int level, string expectedLabel)
    {
        // Given / When
        var milestone = LevelMilestone.ForLevel(level);

        // Then
        milestone.ShouldNotBeNull();
        milestone.Label.ShouldBe(expectedLabel);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllThresholds_When_QueryingThresholds()
    {
        // Given / When
        var thresholds = LevelMilestone.Thresholds;

        // Then
        thresholds.ShouldBe(_expectedThresholds);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectMilestone_When_LevellingUpToMilestoneLevel()
    {
        // Given — level 9 with enough XP to reach level 10
        var level = new Level(9, new ExperiencePoints(0));
        int xpNeeded = Level.CumulativeXpRequired(10) - Level.CumulativeXpRequired(9);

        // When
        var newLevel = level.AddXp(new ExperiencePoints(xpNeeded));

        // Then
        newLevel.Value.ShouldBe(10);
        var milestone = LevelMilestone.ForLevel(newLevel.Value);
        milestone.ShouldNotBeNull();
        milestone.Label.ShouldBe("Double Digits");
    }
}
