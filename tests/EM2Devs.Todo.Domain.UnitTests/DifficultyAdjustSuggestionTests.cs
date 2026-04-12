using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for difficulty auto-adjust suggestion.
/// Maps to: docs/features/progression/experience-points.feature
/// Scenario: "Difficulty rating auto-adjusts for repeated identical tasks"
/// Pure domain calculation: when a task is completed significantly faster or slower
/// than estimated, suggest adjusting the difficulty.
/// </summary>
public sealed class DifficultyAdjustSuggestionTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuggestLowerDifficulty_When_ConsistentlyCompletedMuchFaster()
    {
        // Given — a "Normal" task consistently completed in under 2 minutes
        // when estimated at 30 minutes (variance < -50%)
        TaskDifficulty current = TaskDifficulty.Normal;
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(2);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then — should suggest lowering difficulty
        suggestion.ShouldNotBeNull();
        ((int)suggestion.SuggestedDifficulty).ShouldBeLessThan((int)current);
        suggestion.Reason.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuggestHigherDifficulty_When_ConsistentlyCompletedMuchSlower()
    {
        // Given — a "Normal" task taking much longer than estimated
        // variance > +50%
        TaskDifficulty current = TaskDifficulty.Normal;
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(90);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then — should suggest raising difficulty
        suggestion.ShouldNotBeNull();
        ((int)suggestion.SuggestedDifficulty).ShouldBeGreaterThan((int)current);
        suggestion.Reason.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_CompletionTimeIsCloseToEstimate()
    {
        // Given — completed within reasonable range of estimate (within 50%)
        TaskDifficulty current = TaskDifficulty.Normal;
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(25);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then — no suggestion needed
        suggestion.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuggestBelowTrivial_When_AlreadyTrivial()
    {
        // Given — already at lowest difficulty
        TaskDifficulty current = TaskDifficulty.Trivial;
        TimeEstimate estimated = TimeEstimate.FromMinutes(10);
        TimeEstimate actual = TimeEstimate.FromMinutes(1);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then — cannot go lower, no suggestion
        suggestion.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuggestAboveEpic_When_AlreadyEpic()
    {
        // Given — already at highest difficulty
        TaskDifficulty current = TaskDifficulty.Epic;
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(120);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then — cannot go higher, no suggestion
        suggestion.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuggestEasyOrTrivial_When_NormalCompletedVeryFast()
    {
        // Given — "Normal" task completed in under 2 minutes (estimated 30)
        TaskDifficulty current = TaskDifficulty.Normal;
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(2);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then
        suggestion.ShouldNotBeNull();
        (suggestion.SuggestedDifficulty == TaskDifficulty.Easy || suggestion.SuggestedDifficulty == TaskDifficulty.Trivial).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeExplanation_When_SuggestionMade()
    {
        // Given
        TaskDifficulty current = TaskDifficulty.Hard;
        TimeEstimate estimated = TimeEstimate.FromMinutes(60);
        TimeEstimate actual = TimeEstimate.FromMinutes(5);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then
        suggestion.ShouldNotBeNull();
        suggestion.Reason.ShouldContain("faster");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeSlowerExplanation_When_TakingMuchLonger()
    {
        // Given
        TaskDifficulty current = TaskDifficulty.Easy;
        TimeEstimate estimated = TimeEstimate.FromMinutes(15);
        TimeEstimate actual = TimeEstimate.FromMinutes(60);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then
        suggestion.ShouldNotBeNull();
        suggestion.Reason.ShouldContain("slower");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EstimatedIsNull()
    {
        // When / Then
        Should.Throw<ArgumentNullException>(
            () => DifficultyAdjustSuggestion.Evaluate(TaskDifficulty.Normal, null!, TimeEstimate.FromMinutes(10)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ActualIsNull()
    {
        // When / Then
        Should.Throw<ArgumentNullException>(
            () => DifficultyAdjustSuggestion.Evaluate(TaskDifficulty.Normal, TimeEstimate.FromMinutes(10), null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_CompletedExactlyAtBoundary()
    {
        // Given — exactly 50% faster (boundary case: not exceeded)
        TaskDifficulty current = TaskDifficulty.Normal;
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(15);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then — exactly at -50% boundary, no suggestion (threshold not exceeded)
        suggestion.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_CompletedExactlyAtUpperBoundary()
    {
        // Given — exactly 50% slower (boundary case: not exceeded)
        TaskDifficulty current = TaskDifficulty.Normal;
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(45);

        // When
        DifficultyAdjustSuggestion? suggestion = DifficultyAdjustSuggestion.Evaluate(current, estimated, actual);

        // Then — exactly at +50% boundary, no suggestion
        suggestion.ShouldBeNull();
    }
}
