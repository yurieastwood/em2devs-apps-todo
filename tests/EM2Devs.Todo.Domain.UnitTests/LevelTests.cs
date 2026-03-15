using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Level value object.
/// Maps to: docs/features/progression/levelling.feature
/// Rule: "Levels require logarithmically scaling XP to prevent inflation"
/// </summary>
public sealed class LevelTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartAtLevelOne_When_NewUserCreated()
    {
        // Given / When
        var level = Level.StartingLevel();

        // Then
        level.Value.ShouldBe(1);
        level.CurrentXp.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateLevel_When_ValueIsValid()
    {
        // Given / When
        var level = new Level(5, new ExperiencePoints(200));

        // Then
        level.Value.ShouldBe(5);
        level.CurrentXp.Value.ShouldBe(200);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LevelIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new Level(0, new ExperiencePoints(0)));
        ex.Message.ShouldContain("must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LevelIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new Level(-1, new ExperiencePoints(0)));
        ex.Message.ShouldContain("must be at least 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LevelExceedsMaximum()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Level(Level.MaxLevel + 1, new ExperiencePoints(0)));
        ex.Message.ShouldContain("cannot exceed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUp_When_XpThresholdIsReached()
    {
        // Given — level 3, XP within level = 80, threshold to level 4 = 100
        var level = new Level(3, new ExperiencePoints(80));

        // When — earn 25 XP (80 + 25 = 105, exceeds threshold of 100)
        var result = level.AddXp(new ExperiencePoints(25));

        // Then — should be level 4 with 5 XP carry-over
        result.Value.ShouldBe(4);
        result.CurrentXp.Value.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotLevelUp_When_XpBelowThreshold()
    {
        // Given — level 1 with 0 XP
        var level = Level.StartingLevel();

        // When — earn 10 XP (threshold for level 2 is 50)
        var result = level.AddXp(new ExperiencePoints(10));

        // Then — still level 1, with 10 XP
        result.Value.ShouldBe(1);
        result.CurrentXp.Value.ShouldBe(10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CarryOverExcessXp_When_LevellingUp()
    {
        // Given — level 1 with 45 XP (threshold for level 2 is 50)
        var level = new Level(1, new ExperiencePoints(45));

        // When — earn 10 XP (5 over threshold)
        var result = level.AddXp(new ExperiencePoints(10));

        // Then — level 2 with 5 carry-over
        result.Value.ShouldBe(2);
        result.CurrentXp.Value.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SkipMultipleLevels_When_XpExceedsMultipleThresholds()
    {
        // Given — level 1 with 0 XP
        var level = Level.StartingLevel();

        // When — earn enough XP to skip past level 2 (50) and into level 3
        var result = level.AddXp(new ExperiencePoints(400));

        // Then — should be at least level 3
        result.Value.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(2, 50)]
    [InlineData(5, 300)]
    [InlineData(10, 1000)]
    [InlineData(20, 4000)]
    [InlineData(50, 25000)]
    public void Should_MatchExpectedThreshold_When_CalculatingXpForLevel(
        int level, int expectedCumulativeXp)
    {
        // Given / When
        int threshold = Level.CumulativeXpRequired(level);

        // Then
        threshold.ShouldBe(expectedCumulativeXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemainAtMaxLevel_When_EarningAdditionalXp()
    {
        // Given — at max level
        var level = new Level(Level.MaxLevel, new ExperiencePoints(0));

        // When — earn more XP
        var result = level.AddXp(new ExperiencePoints(500));

        // Then — level stays at max, but XP is tracked
        result.Value.ShouldBe(Level.MaxLevel);
        result.CurrentXp.Value.ShouldBe(500);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnXpToNextLevel_When_NotAtMaxLevel()
    {
        // Given — level 1 with 30 XP (threshold for level 2 is 50)
        var level = new Level(1, new ExperiencePoints(30));

        // When
        int remaining = level.XpToNextLevel();

        // Then — 50 - 30 = 20
        remaining.ShouldBe(20);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZero_When_AtMaxLevelAndQueryingXpToNext()
    {
        // Given — at max level
        var level = new Level(Level.MaxLevel, new ExperiencePoints(100));

        // When
        int remaining = level.XpToNextLevel();

        // Then
        remaining.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullXp()
    {
        // Given
        var level = Level.StartingLevel();

        // When / Then
        Should.Throw<ArgumentNullException>(() => level.AddXp(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatingWithNullXp()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(() => new Level(1, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_CumulativeXpLevelIsLessThanTwo()
    {
        // Given / When / Then
        Should.Throw<ArgumentOutOfRangeException>(() => Level.CumulativeXpRequired(1));
    }
}
