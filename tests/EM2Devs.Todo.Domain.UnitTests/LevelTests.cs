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
        var level = new Level(5, new ExperiencePoints(100));

        // Then
        level.Value.ShouldBe(5);
        level.CurrentXp.Value.ShouldBe(100);
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
    [InlineData(3, 100)]
    [InlineData(4, 200)]
    [InlineData(5, 300)]
    [InlineData(6, 450)]
    [InlineData(7, 600)]
    [InlineData(8, 800)]
    [InlineData(9, 900)]
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
        var ex = Should.Throw<ArgumentOutOfRangeException>(() => Level.CumulativeXpRequired(1));
        ex.Message.ShouldContain("Cumulative XP is only defined for levels 2 through 100");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_CumulativeXpLevelExceedsMax()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => Level.CumulativeXpRequired(Level.MaxLevel + 1));
        ex.Message.ShouldContain("Cumulative XP is only defined for levels 2 through 100");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUpWithZeroCarryOver_When_XpExactlyMatchesThreshold()
    {
        // Given — level 1 with 0 XP, threshold for level 2 is 50
        var level = Level.StartingLevel();

        // When — earn exactly 50 XP
        var result = level.AddXp(new ExperiencePoints(50));

        // Then — level 2 with 0 carry-over (kills totalXp < xpNeeded → <= mutation)
        result.Value.ShouldBe(2);
        result.CurrentXp.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LevelUpToMaxLevel_When_XpReachesMaxThreshold()
    {
        // Given — one level below max
        var level = new Level(Level.MaxLevel - 1, new ExperiencePoints(0));
        int xpNeeded = Level.CumulativeXpRequired(Level.MaxLevel)
                     - Level.CumulativeXpRequired(Level.MaxLevel - 1);

        // When — earn exactly enough to reach max
        var result = level.AddXp(new ExperiencePoints(xpNeeded));

        // Then — at max level, 0 carry-over (kills while < MaxLevel boundary)
        result.Value.ShouldBe(Level.MaxLevel);
        result.CurrentXp.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotLevelPastMax_When_AtMaxLevelMinusOneWithExcessXp()
    {
        // Given — one below max level
        var level = new Level(Level.MaxLevel - 1, new ExperiencePoints(0));
        int xpNeeded = Level.CumulativeXpRequired(Level.MaxLevel)
                     - Level.CumulativeXpRequired(Level.MaxLevel - 1);

        // When — earn more than needed to reach max
        var result = level.AddXp(new ExperiencePoints(xpNeeded + 100));

        // Then — stays at max with carry-over
        result.Value.ShouldBe(Level.MaxLevel);
        result.CurrentXp.Value.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackXp_When_AtMaxLevelExactly()
    {
        // Given — exactly at max level (kills Value >= MaxLevel → > mutation)
        var level = new Level(Level.MaxLevel, new ExperiencePoints(50));

        // When
        var result = level.AddXp(new ExperiencePoints(25));

        // Then — still max, XP accumulated
        result.Value.ShouldBe(Level.MaxLevel);
        result.CurrentXp.Value.ShouldBe(75);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(11)]
    [InlineData(15)]
    [InlineData(19)]
    public void Should_ReturnValidThreshold_When_LevelIsBetween11And20(int level)
    {
        // Given / When — tests levels in the <= 20 interpolation range
        int threshold = Level.CumulativeXpRequired(level);

        // Then — must be between level 10 (1000) and level 20 (4000) boundaries
        threshold.ShouldBeGreaterThan(Level.CumulativeXpRequired(level - 1));
        threshold.ShouldBeLessThanOrEqualTo(4000);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(21)]
    [InlineData(35)]
    [InlineData(49)]
    public void Should_ReturnValidThreshold_When_LevelIsBetween21And50(int level)
    {
        // Given / When — tests levels in the <= 50 interpolation range
        int threshold = Level.CumulativeXpRequired(level);

        // Then — must be between level 20 (4000) and level 50 (25000) boundaries
        threshold.ShouldBeGreaterThan(Level.CumulativeXpRequired(level - 1));
        threshold.ShouldBeLessThanOrEqualTo(25000);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(51)]
    [InlineData(75)]
    [InlineData(99)]
    public void Should_ReturnValidThreshold_When_LevelIsAbove50(int level)
    {
        // Given / When — tests levels in the > 50 interpolation range
        int threshold = Level.CumulativeXpRequired(level);

        // Then — must be monotonically increasing above 25000
        threshold.ShouldBeGreaterThan(Level.CumulativeXpRequired(level - 1));
        threshold.ShouldBeGreaterThan(25000);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCorrectXpForLevel2_When_CalculatingXpForNextLevel()
    {
        // Given — level 1 (kills currentLevel >= 2 → > 2 boundary mutation)
        var level = new Level(1, new ExperiencePoints(0));

        // When
        int xpToNext = level.XpToNextLevel();

        // Then — XpForNextLevel(1) = CumulativeXpRequired(2) - 0 = 50
        xpToNext.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCorrectXpForLevel3_When_AtLevel2()
    {
        // Given — level 2 (verifies cumulative subtraction path for level >= 2)
        var level = new Level(2, new ExperiencePoints(0));

        // When
        int xpToNext = level.XpToNextLevel();

        // Then — XpForNextLevel(2) = CumulativeXpRequired(3) - CumulativeXpRequired(2) = 100 - 50 = 50
        xpToNext.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncreaseMonotonically_When_CalculatingThresholdsAcrossAllRanges()
    {
        // Verify thresholds increase across all range boundaries (kills rounding mutations)
        int previousThreshold = 0;
        for (int lvl = 2; lvl <= Level.MaxLevel; lvl++)
        {
            int threshold = Level.CumulativeXpRequired(lvl);
            threshold.ShouldBeGreaterThan(previousThreshold,
                $"Threshold for level {lvl} ({threshold}) should be greater than level {lvl - 1} ({previousThreshold})");
            previousThreshold = threshold;
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_InitialiseDefaults_When_EfCoreConstructorUsed()
    {
        // Given — EF Core uses the private Level(int) constructor
        var ctor = typeof(Level).GetConstructor(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new[] { typeof(int) },
            null);

        // When
        ctor.ShouldNotBeNull();
        var level = (Level)ctor!.Invoke(new object[] { 7 });

        // Then — Value set, CurrentXp defaulted to 0
        level.Value.ShouldBe(7);
        level.CurrentXp.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroPercent_When_NoXpEarned()
    {
        var level = Level.StartingLevel();
        level.XpProgressPercent().ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCorrectPercent_When_PartialXpEarned()
    {
        var level = new Level(1, new ExperiencePoints(25));
        level.XpProgressPercent().ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Return100Percent_When_AtMaxLevel()
    {
        var level = new Level(Level.MaxLevel, new ExperiencePoints(999));
        level.XpProgressPercent().ShouldBe(100);
    }
}
