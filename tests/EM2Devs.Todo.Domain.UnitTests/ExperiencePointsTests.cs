using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ExperiencePoints value object and XP calculation.
/// Maps to: docs/features/progression/experience-points.feature
/// Rule: "XP is weighted by difficulty, timeliness, and consistency"
/// </summary>
public sealed class ExperiencePointsTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateExperiencePoints_When_ValueIsZero()
    {
        // Given / When
        var xp = new ExperiencePoints(0);

        // Then
        xp.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateExperiencePoints_When_ValueIsPositive()
    {
        // Given / When
        var xp = new ExperiencePoints(50);

        // Then
        xp.Value.ShouldBe(50);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_XpIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new ExperiencePoints(-1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddExperiencePoints_When_CombiningTwoValues()
    {
        // Given
        var xp1 = new ExperiencePoints(30);
        var xp2 = new ExperiencePoints(20);

        // When
        var total = xp1.Add(xp2);

        // Then
        total.Value.ShouldBe(50);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(TaskDifficulty.Trivial, 5, 10)]
    [InlineData(TaskDifficulty.Easy, 10, 20)]
    [InlineData(TaskDifficulty.Normal, 20, 40)]
    [InlineData(TaskDifficulty.Hard, 40, 80)]
    [InlineData(TaskDifficulty.Epic, 80, 150)]
    public void Should_ReturnXpWithinRange_When_CalculatingBaseXpForDifficulty(
        TaskDifficulty difficulty, int minXp, int maxXp)
    {
        // Given / When
        var xp = ExperiencePoints.BaseForDifficulty(difficulty);

        // Then
        xp.Value.ShouldBeGreaterThanOrEqualTo(minXp);
        xp.Value.ShouldBeLessThanOrEqualTo(maxXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullXp()
    {
        // Given
        var xp = new ExperiencePoints(10);

        // When / Then
        Should.Throw<ArgumentNullException>(() => xp.Add(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentException_When_DifficultyIsInvalid()
    {
        // Given
        var invalidDifficulty = (TaskDifficulty)999;

        // When / Then
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => ExperiencePoints.BaseForDifficulty(invalidDifficulty));
        ex.Message.ShouldContain("Unknown task difficulty");
    }
}
