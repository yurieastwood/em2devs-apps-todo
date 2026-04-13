using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for EngagementUnlockRegistry.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// — "Features unlock at specific engagement thresholds"
/// </summary>
public sealed class EngagementUnlockRegistryTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockQuests_When_FiveTasksCreated()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 5, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        unlocked.ShouldContain(UnlockableFeature.Quests);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnlockQuests_When_BelowThreshold()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 4, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        unlocked.ShouldNotContain(UnlockableFeature.Quests);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockXp_When_TenTasksCompleted()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 10, currentLevel: 1, questsCompleted: 0);

        unlocked.ShouldContain(UnlockableFeature.BasicXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnlockXp_When_BelowThreshold()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 9, currentLevel: 1, questsCompleted: 0);

        unlocked.ShouldNotContain(UnlockableFeature.BasicXp);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockSkillTrees_When_Level3()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 3, questsCompleted: 0);

        unlocked.ShouldContain(UnlockableFeature.SkillTrees);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnlockSkillTrees_When_BelowLevel3()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 2, questsCompleted: 0);

        unlocked.ShouldNotContain(UnlockableFeature.SkillTrees);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockTitles_When_Level5()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 5, questsCompleted: 0);

        unlocked.ShouldContain(UnlockableFeature.Titles);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnlockTitles_When_BelowLevel5()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 4, questsCompleted: 0);

        unlocked.ShouldNotContain(UnlockableFeature.Titles);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockEpics_When_ThreeQuestsCompleted()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 1, questsCompleted: 3);

        unlocked.ShouldContain(UnlockableFeature.DailyBrief);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnlockEpics_When_BelowThreshold()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 1, questsCompleted: 2);

        unlocked.ShouldNotContain(UnlockableFeature.DailyBrief);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockAccountabilityPartner_When_Level7()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 7, questsCompleted: 0);

        unlocked.ShouldContain(UnlockableFeature.AccountabilityPartners);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotUnlockAccountabilityPartner_When_BelowLevel7()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 6, questsCompleted: 0);

        unlocked.ShouldNotContain(UnlockableFeature.AccountabilityPartners);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnlockMultipleFeatures_When_MultipleThresholdsMet()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 5, tasksCompleted: 10, currentLevel: 7, questsCompleted: 3);

        unlocked.ShouldContain(UnlockableFeature.Quests);
        unlocked.ShouldContain(UnlockableFeature.BasicXp);
        unlocked.ShouldContain(UnlockableFeature.SkillTrees);
        unlocked.ShouldContain(UnlockableFeature.Titles);
        unlocked.ShouldContain(UnlockableFeature.DailyBrief);
        unlocked.ShouldContain(UnlockableFeature.AccountabilityPartners);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnEmpty_When_NoThresholdsMet()
    {
        IReadOnlyList<UnlockableFeature> unlocked = EngagementUnlockRegistry.EvaluateUnlocks(
            tasksCreated: 0, tasksCompleted: 0, currentLevel: 1, questsCompleted: 0);

        unlocked.ShouldBeEmpty();
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(UnlockableFeature.Quests, "5 tasks created")]
    [InlineData(UnlockableFeature.BasicXp, "10 tasks completed")]
    [InlineData(UnlockableFeature.SkillTrees, "Level 3")]
    [InlineData(UnlockableFeature.Titles, "Level 5")]
    [InlineData(UnlockableFeature.DailyBrief, "3 quests completed")]
    [InlineData(UnlockableFeature.AccountabilityPartners, "Level 7")]
    public void Should_ReturnCorrectDescription_When_GettingThresholdDescription(
        UnlockableFeature feature, string expectedDescription)
    {
        string description = EngagementUnlockRegistry.GetThresholdDescription(feature);
        description.ShouldBe(expectedDescription);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_UndefinedFeatureDescription()
    {
        ArgumentOutOfRangeException ex = Should.Throw<ArgumentOutOfRangeException>(
            () => EngagementUnlockRegistry.GetThresholdDescription(UnlockableFeature.Leaderboards));
        ex.Message.ShouldContain("No engagement threshold");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeCorrectConstants()
    {
        EngagementUnlockRegistry.QuestsTasksCreatedThreshold.ShouldBe(5);
        EngagementUnlockRegistry.XpTasksCompletedThreshold.ShouldBe(10);
        EngagementUnlockRegistry.SkillTreesLevelThreshold.ShouldBe(3);
        EngagementUnlockRegistry.TitlesLevelThreshold.ShouldBe(5);
        EngagementUnlockRegistry.EpicsQuestsCompletedThreshold.ShouldBe(3);
        EngagementUnlockRegistry.AccountabilityPartnerLevelThreshold.ShouldBe(7);
    }
}
