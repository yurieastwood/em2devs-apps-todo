using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for SkillTree and related value objects.
/// Maps to: docs/features/progression/skill-trees.feature
/// Rule: "Skill trees are discovered and unlocked through natural behaviour patterns"
/// Rule: "Each skill tree has multiple tiers that unlock through sustained behaviour"
/// </summary>
public sealed class SkillTreeTests
{
    // --- SkillTreeType and category mapping ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("creative", SkillTreeType.Creator)]
    [InlineData("health", SkillTreeType.Guardian)]
    [InlineData("fitness", SkillTreeType.Guardian)]
    [InlineData("learning", SkillTreeType.Scholar)]
    [InlineData("study", SkillTreeType.Scholar)]
    [InlineData("work", SkillTreeType.Architect)]
    [InlineData("career", SkillTreeType.Architect)]
    [InlineData("social", SkillTreeType.Connector)]
    [InlineData("people", SkillTreeType.Connector)]
    [InlineData("home", SkillTreeType.Steward)]
    [InlineData("organising", SkillTreeType.Steward)]
    [InlineData("side-project", SkillTreeType.Builder)]
    public void Should_MapCategoryToTreeType_When_CategoryIsKnown(
        string category, SkillTreeType expectedType)
    {
        // Given / When
        bool found = SkillTreeDiscovery.TryGetTreeType(category, out SkillTreeType treeType);

        // Then
        found.ShouldBeTrue();
        treeType.ShouldBe(expectedType);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_CategoryIsUnknown()
    {
        // Given / When
        bool found = SkillTreeDiscovery.TryGetTreeType("unknown-category", out _);

        // Then
        found.ShouldBeFalse();
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(SkillTreeType.Creator, 15)]
    [InlineData(SkillTreeType.Guardian, 15)]
    [InlineData(SkillTreeType.Scholar, 15)]
    [InlineData(SkillTreeType.Architect, 20)]
    [InlineData(SkillTreeType.Connector, 15)]
    [InlineData(SkillTreeType.Steward, 15)]
    [InlineData(SkillTreeType.Builder, 10)]
    public void Should_ReturnCorrectDiscoveryThreshold_When_QueryingTreeType(
        SkillTreeType treeType, int expectedThreshold)
    {
        // Given / When
        int threshold = SkillTreeDiscovery.DiscoveryThreshold(treeType);

        // Then
        threshold.ShouldBe(expectedThreshold);
    }

    // --- SkillTier ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Should_CreateSkillTier_When_ValueIsValid(int value)
    {
        // Given / When
        var tier = new SkillTier(value);

        // Then
        tier.Value.ShouldBe(value);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TierIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new SkillTier(0));
        ex.Message.ShouldContain("must be between 1 and");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TierExceedsMax()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new SkillTier(SkillTier.MaxTier + 1));
        ex.Message.ShouldContain("must be between 1 and");
    }

    // --- SkillTree entity ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSkillTree_When_Discovered()
    {
        // Given / When
        var tree = SkillTree.Discover(SkillTreeType.Builder);

        // Then
        tree.Type.ShouldBe(SkillTreeType.Builder);
        tree.CurrentTier.Value.ShouldBe(1);
        tree.TasksCompletedInTier.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementTaskCount_When_RecordingProgress()
    {
        // Given
        var tree = SkillTree.Discover(SkillTreeType.Builder);

        // When
        var result = tree.RecordTaskCompletion();

        // Then
        result.TasksCompletedInTier.ShouldBe(1);
        result.CurrentTier.Value.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdvanceToTier2_When_ThresholdReached()
    {
        // Given — Builder at tier 1, need 30 tasks for tier 2
        var tree = new SkillTree(
            SkillTreeType.Builder,
            new SkillTier(1),
            SkillTree.TasksRequiredForTier(2) - 1);

        // When — complete one more task
        var result = tree.RecordTaskCompletion();

        // Then — tier advances to 2, task count resets
        result.CurrentTier.Value.ShouldBe(2);
        result.TasksCompletedInTier.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemainAtMaxTier_When_AlreadyAtMax()
    {
        // Given — at max tier
        var tree = new SkillTree(
            SkillTreeType.Creator,
            new SkillTier(SkillTier.MaxTier),
            10);

        // When — complete more tasks
        var result = tree.RecordTaskCompletion();

        // Then — stays at max, tasks still counted
        result.CurrentTier.Value.ShouldBe(SkillTier.MaxTier);
        result.TasksCompletedInTier.ShouldBe(11);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTasksToNextTier_When_NotAtMax()
    {
        // Given — tier 1 with 10 tasks completed
        var tree = new SkillTree(SkillTreeType.Scholar, new SkillTier(1), 10);

        // When
        int remaining = tree.TasksToNextTier();

        // Then
        int required = SkillTree.TasksRequiredForTier(2);
        remaining.ShouldBe(required - 10);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZero_When_AtMaxTierAndQueryingTasksToNext()
    {
        // Given — at max tier
        var tree = new SkillTree(
            SkillTreeType.Architect,
            new SkillTier(SkillTier.MaxTier),
            5);

        // When
        int remaining = tree.TasksToNextTier();

        // Then
        remaining.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TasksCompletedIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new SkillTree(SkillTreeType.Builder, new SkillTier(1), -1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(2, 30)]
    [InlineData(3, 60)]
    public void Should_ReturnCorrectTasksRequired_When_QueryingTierThreshold(
        int tier, int expectedTasks)
    {
        // Given / When
        int required = SkillTree.TasksRequiredForTier(tier);

        // Then
        required.ShouldBe(expectedTasks);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_TierThresholdIsForTier1OrLess()
    {
        // Given / When / Then
        Should.Throw<ArgumentOutOfRangeException>(
            () => SkillTree.TasksRequiredForTier(1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_InvalidDiscoveryThreshold()
    {
        // Given / When / Then
        Should.Throw<ArgumentOutOfRangeException>(
            () => SkillTreeDiscovery.DiscoveryThreshold((SkillTreeType)999));
    }
}
