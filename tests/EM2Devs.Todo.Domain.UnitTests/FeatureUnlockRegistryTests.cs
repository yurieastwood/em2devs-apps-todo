using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for FeatureUnlockRegistry.
/// Maps to: docs/features/progression/levelling.feature
/// Scenario Outline: "Progressive feature unlocks by level"
/// </summary>
public sealed class FeatureUnlockRegistryTests
{
    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(1, UnlockableFeature.Tasks)]
    [InlineData(1, UnlockableFeature.Quests)]
    [InlineData(1, UnlockableFeature.BasicXp)]
    [InlineData(3, UnlockableFeature.SkillTrees)]
    [InlineData(5, UnlockableFeature.Titles)]
    [InlineData(5, UnlockableFeature.DailyBrief)]
    [InlineData(7, UnlockableFeature.AccountabilityPartners)]
    [InlineData(10, UnlockableFeature.Leaderboards)]
    [InlineData(10, UnlockableFeature.ChallengeMode)]
    [InlineData(15, UnlockableFeature.InsightCards)]
    [InlineData(20, UnlockableFeature.AdvancedAnalytics)]
    public void Should_UnlockFeature_When_ReachingRequiredLevel(int level, UnlockableFeature feature)
    {
        // Given / When
        var unlockedFeatures = FeatureUnlockRegistry.GetUnlockedFeatures(level);

        // Then
        unlockedFeatures.ShouldContain(feature);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(2, UnlockableFeature.SkillTrees)]
    [InlineData(4, UnlockableFeature.Titles)]
    [InlineData(6, UnlockableFeature.AccountabilityPartners)]
    [InlineData(9, UnlockableFeature.Leaderboards)]
    [InlineData(14, UnlockableFeature.InsightCards)]
    [InlineData(19, UnlockableFeature.AdvancedAnalytics)]
    public void Should_NotUnlockFeature_When_BelowRequiredLevel(int level, UnlockableFeature feature)
    {
        // Given / When
        var unlockedFeatures = FeatureUnlockRegistry.GetUnlockedFeatures(level);

        // Then
        unlockedFeatures.ShouldNotContain(feature);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(1, 3)]
    [InlineData(3, 4)]
    [InlineData(5, 6)]
    [InlineData(7, 7)]
    [InlineData(10, 9)]
    [InlineData(15, 10)]
    [InlineData(20, 11)]
    public void Should_ReturnCorrectFeatureCount_When_AtGivenLevel(int level, int expectedCount)
    {
        // Given / When
        var unlockedFeatures = FeatureUnlockRegistry.GetUnlockedFeatures(level);

        // Then
        unlockedFeatures.Count.ShouldBe(expectedCount);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNewlyUnlockedFeatures_When_LevellingUp()
    {
        // Given / When
        var newFeatures = FeatureUnlockRegistry.GetNewlyUnlockedFeatures(3);

        // Then — level 3 unlocks SkillTrees
        newFeatures.ShouldContain(UnlockableFeature.SkillTrees);
        newFeatures.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMultipleNewFeatures_When_LevelUnlocksMultiple()
    {
        // Given / When — level 10 unlocks Leaderboards and ChallengeMode
        var newFeatures = FeatureUnlockRegistry.GetNewlyUnlockedFeatures(10);

        // Then
        newFeatures.ShouldContain(UnlockableFeature.Leaderboards);
        newFeatures.ShouldContain(UnlockableFeature.ChallengeMode);
        newFeatures.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmpty_When_NoNewFeaturesAtLevel()
    {
        // Given / When — level 2 doesn't unlock anything new
        var newFeatures = FeatureUnlockRegistry.GetNewlyUnlockedFeatures(2);

        // Then
        newFeatures.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnBaseFeatures_When_AtLevelOne()
    {
        // Given / When
        var newFeatures = FeatureUnlockRegistry.GetNewlyUnlockedFeatures(1);

        // Then — level 1 has Tasks, Quests, BasicXp
        newFeatures.ShouldContain(UnlockableFeature.Tasks);
        newFeatures.ShouldContain(UnlockableFeature.Quests);
        newFeatures.ShouldContain(UnlockableFeature.BasicXp);
        newFeatures.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeAllPreviousFeatures_When_AtHigherLevel()
    {
        // Given / When — at level 20, should have ALL features
        var features = FeatureUnlockRegistry.GetUnlockedFeatures(20);

        // Then
        foreach (UnlockableFeature flag in Enum.GetValues<UnlockableFeature>())
        {
            features.ShouldContain(flag);
        }
    }
}
