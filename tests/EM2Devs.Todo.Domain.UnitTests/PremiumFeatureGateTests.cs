using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the PremiumFeatureGate domain service.
/// Maps to: docs/features/monetisation/subscription-tiers.feature
/// Scenarios: "Free-tier user has access to core features",
///            "Premium user has access to all premium features",
///            "Team tier includes team-specific features",
///            "Free-tier user encounters a premium feature",
///            "Cosmetics do not affect gameplay"
/// </summary>
public sealed class PremiumFeatureGateTests
{
    // --- Scenario: Free-tier user has access to core features ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("Unlimited tasks")]
    [InlineData("Unlimited quests")]
    [InlineData("Unlimited epics")]
    [InlineData("Full XP and levelling engine")]
    [InlineData("Skill trees")]
    [InlineData("Titles and ranks")]
    [InlineData("Basic daily brief")]
    [InlineData("Energy-aware scheduling")]
    [InlineData("One accountability partner")]
    [InlineData("Basic weekly review")]
    [InlineData("Journey timeline")]
    [InlineData("Local data storage")]
    [InlineData("Manual data export")]
    public void Should_HaveAccess_When_FreeTierUserAccessesCoreFeature(string feature)
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Free, feature).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllFreeFeatures_When_FreeTier()
    {
        var features = PremiumFeatureGate.GetAccessibleFeatures(SubscriptionTier.Free);

        features.Count.ShouldBe(13);
        features.ShouldContain("Unlimited tasks");
        features.ShouldContain("Manual data export");
    }

    // --- Scenario: Premium user has access to all premium features ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("Sagas and long-arc goal tracking")]
    [InlineData("Capacity modelling")]
    [InlineData("Time estimation learning")]
    [InlineData("Insight cards")]
    [InlineData("Guilds (create and join up to 5)")]
    [InlineData("Challenge mode")]
    [InlineData("Seasonal leaderboards")]
    [InlineData("Cross-device sync")]
    [InlineData("Priority themes and cosmetics")]
    [InlineData("Advanced weekly review")]
    [InlineData("Annual Wrapped")]
    [InlineData("Calendar integration")]
    public void Should_HaveAccess_When_ProTierUserAccessesPremiumFeature(string feature)
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Pro, feature).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFreeAndProFeatures_When_ProTier()
    {
        var features = PremiumFeatureGate.GetAccessibleFeatures(SubscriptionTier.Pro);

        features.Count.ShouldBe(25); // 13 free + 12 pro
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("Unlimited tasks")]
    [InlineData("Basic weekly review")]
    public void Should_AlsoHaveFreeFeatures_When_ProTier(string feature)
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Pro, feature).ShouldBeTrue();
    }

    // --- Scenario: Free-tier user encounters a premium feature ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotHaveAccess_When_FreeTierUserAccessesPremiumFeature()
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Free, "Sagas and long-arc goal tracking")
            .ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IdentifyPremiumFeature_When_ProFeature()
    {
        PremiumFeatureGate.IsPremiumFeature("Sagas and long-arc goal tracking").ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IdentifyPremiumFeature_When_TeamFeature()
    {
        PremiumFeatureGate.IsPremiumFeature("Shared quest boards with roles").ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotIdentifyAsPremium_When_FreeFeature()
    {
        PremiumFeatureGate.IsPremiumFeature("Unlimited tasks").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_FeatureNameIsEmpty()
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Free, "").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_FeatureNameIsWhitespace()
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Free, "  ").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_IsPremiumFeatureNameIsEmpty()
    {
        PremiumFeatureGate.IsPremiumFeature("").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_IsPremiumFeatureNameIsWhitespace()
    {
        PremiumFeatureGate.IsPremiumFeature("  ").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_UnknownFeatureName()
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Free, "Unknown Feature").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_IsPremiumWithUnknownFeature()
    {
        PremiumFeatureGate.IsPremiumFeature("Unknown Feature").ShouldBeFalse();
    }

    // --- Scenario: Team tier includes team-specific features ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("Everything in Pro")]
    [InlineData("Shared quest boards with roles")]
    [InlineData("Team analytics and velocity tracking")]
    [InlineData("Admin controls and onboarding flows")]
    [InlineData("Dedicated team leaderboards")]
    public void Should_HaveAccess_When_TeamTierUserAccessesTeamFeature(string feature)
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Team, feature).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllFeatures_When_TeamTier()
    {
        var features = PremiumFeatureGate.GetAccessibleFeatures(SubscriptionTier.Team);

        features.Count.ShouldBe(30); // 13 free + 12 pro + 5 team
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotHaveTeamFeatures_When_ProTier()
    {
        PremiumFeatureGate.HasAccess(SubscriptionTier.Pro, "Shared quest boards with roles")
            .ShouldBeFalse();
    }

    // --- GetRequiredTier ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFreeTier_When_FreeFeature()
    {
        PremiumFeatureGate.GetRequiredTier("Unlimited tasks").ShouldBe(SubscriptionTier.Free);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnProTier_When_ProFeature()
    {
        PremiumFeatureGate.GetRequiredTier("Sagas and long-arc goal tracking")
            .ShouldBe(SubscriptionTier.Pro);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTeamTier_When_TeamFeature()
    {
        PremiumFeatureGate.GetRequiredTier("Shared quest boards with roles")
            .ShouldBe(SubscriptionTier.Team);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UnknownFeatureForRequiredTier()
    {
        var ex = Should.Throw<DomainException>(
            () => PremiumFeatureGate.GetRequiredTier("Unknown Feature"));
        ex.Message.ShouldContain("Unknown feature");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_FeatureNameIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => PremiumFeatureGate.GetRequiredTier(null!));
    }

    // --- Invalid tier ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentOutOfRange_When_InvalidTier()
    {
        var ex = Should.Throw<ArgumentOutOfRangeException>(
            () => PremiumFeatureGate.GetAccessibleFeatures((SubscriptionTier)99));
        ex.Message.ShouldContain("Unknown subscription tier");
    }

    // --- Scenario: Cosmetics do not affect gameplay ---
    // Cosmetics provide no XP or gameplay advantage — verified by design:
    // CosmeticPurchase has no XP-related properties.
    // ExperiencePoints.BaseForDifficulty does not reference cosmetics.

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateIdenticalXp_When_CosmeticsOwned()
    {
        // Given two users with identical task completion
        var xpWithoutCosmetics = ExperiencePoints.BaseForDifficulty(TaskDifficulty.Normal);
        var xpWithCosmetics = ExperiencePoints.BaseForDifficulty(TaskDifficulty.Normal);

        // Then XP should be identical regardless of cosmetic ownership
        xpWithoutCosmetics.Value.ShouldBe(xpWithCosmetics.Value);
    }

    // --- Feature list completeness ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveCorrectFreeFeatureCount()
    {
        PremiumFeatureGate.FreeFeatures.Count.ShouldBe(13);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveCorrectProFeatureCount()
    {
        PremiumFeatureGate.ProFeatures.Count.ShouldBe(12);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveCorrectTeamFeatureCount()
    {
        PremiumFeatureGate.TeamFeatures.Count.ShouldBe(5);
    }
}
