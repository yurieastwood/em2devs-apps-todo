using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for DifficultyWeight mapping.
/// Tests encode difficulty weighting from capacity-modelling.feature.
/// </summary>
public sealed class DifficultyWeightTests
{
    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(TaskDifficulty.Trivial, 1)]
    [InlineData(TaskDifficulty.Easy, 2)]
    [InlineData(TaskDifficulty.Normal, 3)]
    [InlineData(TaskDifficulty.Hard, 5)]
    [InlineData(TaskDifficulty.Epic, 8)]
    public void Should_ReturnCorrectWeight_When_DifficultyMapped(TaskDifficulty difficulty, int expectedWeight)
    {
        // When
        int weight = DifficultyWeight.For(difficulty);

        // Then
        weight.ShouldBe(expectedWeight);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNormalWeight_When_UnknownDifficultyUsed()
    {
        // Given an undefined enum value
        var unknownDifficulty = (TaskDifficulty)42;

        // When
        int weight = DifficultyWeight.For(unknownDifficulty);

        // Then
        weight.ShouldBe(DifficultyWeight.Normal);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveCorrectConstantValues_When_Accessed()
    {
        DifficultyWeight.Trivial.ShouldBe(1);
        DifficultyWeight.Easy.ShouldBe(2);
        DifficultyWeight.Normal.ShouldBe(3);
        DifficultyWeight.Hard.ShouldBe(5);
        DifficultyWeight.Epic.ShouldBe(8);
    }
}
