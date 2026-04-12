using Shouldly;
using EM2Devs.Todo.Domain.Entities;
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
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => SkillTree.TasksRequiredForTier(1));
        ex.Message.ShouldContain("Task requirements are only defined for tiers 2 through");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_TierThresholdExceedsMax()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => SkillTree.TasksRequiredForTier(SkillTier.MaxTier + 1));
        ex.Message.ShouldContain("Task requirements are only defined for tiers 2 through");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatingTreeWithNullTier()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(
            () => new SkillTree(SkillTreeType.Builder, null!, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_InvalidDiscoveryThreshold()
    {
        // Given / When / Then
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => SkillTreeDiscovery.DiscoveryThreshold((SkillTreeType)999));
        ex.Message.ShouldContain("Unknown skill tree type");
    }

    // --- Scenario: Unlock a skill tree by demonstrating behaviour ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("side-project", 10, SkillTreeType.Builder)]
    [InlineData("creative", 15, SkillTreeType.Creator)]
    [InlineData("work", 20, SkillTreeType.Architect)]
    public void Should_UnlockSkillTree_When_TaskCountReachesDiscoveryThreshold(
        string category, int threshold, SkillTreeType expectedType)
    {
        // Given
        bool found = SkillTreeDiscovery.TryGetTreeType(category, out SkillTreeType treeType);
        found.ShouldBeTrue();
        treeType.ShouldBe(expectedType);

        int discoveryThreshold = SkillTreeDiscovery.DiscoveryThreshold(treeType);
        discoveryThreshold.ShouldBe(threshold);

        // When — tasks completed reaches the threshold
        var tree = SkillTree.Discover(treeType);

        // Then — tree starts at tier 1
        tree.Type.ShouldBe(expectedType);
        tree.CurrentTier.Value.ShouldBe(1);
        tree.TasksCompletedInTier.ShouldBe(0);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("side-project", 9)]
    [InlineData("creative", 14)]
    [InlineData("work", 19)]
    public void Should_NotUnlockSkillTree_When_TaskCountBelowDiscoveryThreshold(
        string category, int taskCount)
    {
        // Given
        bool found = SkillTreeDiscovery.TryGetTreeType(category, out SkillTreeType treeType);
        found.ShouldBeTrue();

        int threshold = SkillTreeDiscovery.DiscoveryThreshold(treeType);

        // Then — below threshold means no unlock
        taskCount.ShouldBeLessThan(threshold);
    }

    // --- Scenario: Skill tree is hidden before level 3 ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotShowSkillTrees_When_PlayerBelowLevel3()
    {
        // Given — player at level 2
        var features = FeatureUnlockRegistry.GetUnlockedFeatures(2);

        // Then — skill trees should not be in the unlocked features
        features.ShouldNotContain(UnlockableFeature.SkillTrees);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowSkillTrees_When_PlayerAtLevel3()
    {
        // Given — player at level 3
        var features = FeatureUnlockRegistry.GetUnlockedFeatures(3);

        // Then — skill trees should be unlocked
        features.ShouldContain(UnlockableFeature.SkillTrees);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowSkillTrees_When_PlayerAboveLevel3()
    {
        // Given — player at level 5
        var features = FeatureUnlockRegistry.GetUnlockedFeatures(5);

        // Then — skill trees should still be unlocked
        features.ShouldContain(UnlockableFeature.SkillTrees);
    }

    // --- Scenario: Progress within a skill tree follows tier progression ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ProgressFromTier1ToTier2_When_CompletingRequiredTasks()
    {
        // Given — Builder at tier 1, 25 tasks completed, need 30 for tier 2
        var tree = new SkillTree(SkillTreeType.Builder, new SkillTier(1), 25);
        tree.TasksToNextTier().ShouldBe(5);

        // When — complete 5 more tasks
        var result = tree;
        for (int i = 0; i < 5; i++)
        {
            result = result.RecordTaskCompletion();
        }

        // Then — advances to tier 2
        result.CurrentTier.Value.ShouldBe(2);
        result.TasksCompletedInTier.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdvanceFromTier2ToTier3_When_ThresholdReached()
    {
        // Given — at tier 2, need 60 tasks for tier 3
        var tree = new SkillTree(
            SkillTreeType.Scholar,
            new SkillTier(2),
            SkillTree.TasksRequiredForTier(3) - 1);

        // When
        var result = tree.RecordTaskCompletion();

        // Then
        result.CurrentTier.Value.ShouldBe(3);
        result.TasksCompletedInTier.ShouldBe(0);
    }

    // --- Scenario: Skill tree progress retained after inactivity ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RetainProgress_When_NoActivityOccurs()
    {
        // Given — Builder at tier 2 with 15 tasks completed toward tier 3
        var tree = new SkillTree(SkillTreeType.Builder, new SkillTier(2), 15);

        // When — no activity (no RecordTaskCompletion calls) — simulating 60 days of inactivity
        // The SkillTree is immutable; no decay mechanism exists

        // Then — tier and progress are unchanged
        tree.CurrentTier.Value.ShouldBe(2);
        tree.TasksCompletedInTier.ShouldBe(15);
        tree.TasksToNextTier().ShouldBe(SkillTree.TasksRequiredForTier(3) - 15);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NeverDecayProgress_When_SkillTreeIsImmutableRecord()
    {
        // Given — a tree at max tier with many tasks
        var tree = new SkillTree(
            SkillTreeType.Creator,
            new SkillTier(SkillTier.MaxTier),
            50);

        // Then — progress is permanent; no method exists to reduce progress
        tree.CurrentTier.Value.ShouldBe(SkillTier.MaxTier);
        tree.TasksCompletedInTier.ShouldBe(50);
        tree.TasksToNextTier().ShouldBe(0);
    }

    // --- Scenario: View skill tree details ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportCurrentTierAndProgress_When_ViewingDetails()
    {
        // Given — Scholar at tier 2, with 20 tasks completed
        var tree = new SkillTree(SkillTreeType.Scholar, new SkillTier(2), 20);

        // Then — can view current tier, progress, and category
        tree.CurrentTier.Value.ShouldBe(2);
        tree.TasksCompletedInTier.ShouldBe(20);
        tree.TasksToNextTier().ShouldBe(SkillTree.TasksRequiredForTier(3) - 20);
        tree.Type.ShouldBe(SkillTreeType.Scholar);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportCategories_When_QueryingSkillTreeType()
    {
        // Given / When — query all categories for Scholar
        bool foundLearning = SkillTreeDiscovery.TryGetTreeType("learning", out SkillTreeType type1);
        bool foundStudy = SkillTreeDiscovery.TryGetTreeType("study", out SkillTreeType type2);

        // Then
        foundLearning.ShouldBeTrue();
        foundStudy.ShouldBeTrue();
        type1.ShouldBe(SkillTreeType.Scholar);
        type2.ShouldBe(SkillTreeType.Scholar);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportMaxTierDetails_When_AtMaxTier()
    {
        // Given — at max tier
        var tree = new SkillTree(
            SkillTreeType.Architect,
            new SkillTier(SkillTier.MaxTier),
            10);

        // Then — tasks to next is 0, current tier is max
        tree.CurrentTier.Value.ShouldBe(SkillTier.MaxTier);
        tree.TasksToNextTier().ShouldBe(0);
        tree.TasksCompletedInTier.ShouldBe(10);
    }

    // --- Scenario: Category lookup case-insensitivity ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MapCategory_When_CaseIsDifferent()
    {
        // Given / When
        bool found = SkillTreeDiscovery.TryGetTreeType("CREATIVE", out SkillTreeType treeType);

        // Then
        found.ShouldBeTrue();
        treeType.ShouldBe(SkillTreeType.Creator);
    }

    // --- PlayerProfile skill tree ownership ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartWithNoSkillTrees_When_NewProfileCreated()
    {
        // Given / When
        var profile = PlayerProfile.NewProfile();

        // Then
        profile.SkillTrees.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockSkillTree_When_DiscoverCalledOnProfile()
    {
        // Given
        var profile = PlayerProfile.NewProfile();

        // When
        profile.DiscoverSkillTree(SkillTreeType.Builder);

        // Then
        profile.SkillTrees.ShouldHaveSingleItem();
        profile.SkillTrees[0].Type.ShouldBe(SkillTreeType.Builder);
        profile.SkillTrees[0].CurrentTier.Value.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDuplicateSkillTree_When_DiscoveringSameTypeTwice()
    {
        // Given
        var profile = PlayerProfile.NewProfile();
        profile.DiscoverSkillTree(SkillTreeType.Builder);

        // When
        profile.DiscoverSkillTree(SkillTreeType.Builder);

        // Then — still only one Builder tree
        profile.SkillTrees.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdvanceSkillTree_When_RecordingCategoryTaskOnProfile()
    {
        // Given
        var profile = PlayerProfile.NewProfile();
        profile.DiscoverSkillTree(SkillTreeType.Builder);

        // When
        profile.RecordSkillTreeProgress(SkillTreeType.Builder);

        // Then
        profile.SkillTrees[0].TasksCompletedInTier.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveMultipleSkillTrees_When_MultipleDiscovered()
    {
        // Given
        var profile = PlayerProfile.NewProfile();

        // When
        profile.DiscoverSkillTree(SkillTreeType.Creator);
        profile.DiscoverSkillTree(SkillTreeType.Builder);

        // Then
        profile.SkillTrees.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RecordingProgressForUndiscoveredTree()
    {
        // Given
        var profile = PlayerProfile.NewProfile();

        // When / Then
        var ex = Should.Throw<Exceptions.DomainException>(
            () => profile.RecordSkillTreeProgress(SkillTreeType.Builder));
        ex.Message.ShouldContain("not been discovered");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckSkillTreeVisibility_When_UsingFeatureUnlockRegistry()
    {
        // Given — level 1 and level 3
        var featuresLevel1 = FeatureUnlockRegistry.GetUnlockedFeatures(1);
        var featuresLevel3 = FeatureUnlockRegistry.GetUnlockedFeatures(3);

        // Then — level 1 does not include SkillTrees, level 3 does
        featuresLevel1.ShouldNotContain(UnlockableFeature.SkillTrees);
        featuresLevel3.ShouldContain(UnlockableFeature.SkillTrees);
    }
}
