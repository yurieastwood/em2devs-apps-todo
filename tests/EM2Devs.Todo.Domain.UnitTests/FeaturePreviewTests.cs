using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for FeaturePreview value object.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// — "User can manually explore features early"
/// </summary>
public sealed class FeaturePreviewTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateFeaturePreview_When_ValidParameters()
    {
        // Given / When
        FeaturePreview preview = new(
            UnlockableFeature.Quests,
            "Group related tasks into quests",
            "5 tasks created");

        // Then
        preview.Feature.ShouldBe(UnlockableFeature.Quests);
        preview.Description.ShouldBe("Group related tasks into quests");
        preview.UnlockRequirement.ShouldBe("5 tasks created");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyDescription()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new FeaturePreview(UnlockableFeature.Quests, "", "5 tasks"));
        ex.Message.ShouldContain("description");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WhitespaceDescription()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new FeaturePreview(UnlockableFeature.Quests, "   ", "5 tasks"));
        ex.Message.ShouldContain("description");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyUnlockRequirement()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new FeaturePreview(UnlockableFeature.Quests, "Description", ""));
        ex.Message.ShouldContain("unlock requirement");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WhitespaceUnlockRequirement()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new FeaturePreview(UnlockableFeature.Quests, "Description", "   "));
        ex.Message.ShouldContain("unlock requirement");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullDescription()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new FeaturePreview(UnlockableFeature.Quests, null!, "5 tasks"));
        ex.Message.ShouldContain("description");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullUnlockRequirement()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new FeaturePreview(UnlockableFeature.Quests, "Description", null!));
        ex.Message.ShouldContain("unlock requirement");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SupportEquality_When_SameValues()
    {
        FeaturePreview a = new(UnlockableFeature.Quests, "Desc", "Req");
        FeaturePreview b = new(UnlockableFeature.Quests, "Desc", "Req");
        a.ShouldBe(b);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentFeature()
    {
        FeaturePreview a = new(UnlockableFeature.Quests, "Desc", "Req");
        FeaturePreview b = new(UnlockableFeature.BasicXp, "Desc", "Req");
        a.ShouldNotBe(b);
    }
}
