using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Tests for EngagementThreshold value object validation.
/// Maps to: docs/features/onboarding/progressive-disclosure.feature
/// </summary>
public sealed class EngagementThresholdTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateThreshold_When_ValidParameters()
    {
        EngagementThreshold threshold = new("5 tasks created", UnlockableFeature.Quests);

        threshold.ThresholdKey.ShouldBe("5 tasks created");
        threshold.Feature.ShouldBe(UnlockableFeature.Quests);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyThresholdKey()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new EngagementThreshold("", UnlockableFeature.Quests));
        ex.Message.ShouldContain("Threshold key");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WhitespaceThresholdKey()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new EngagementThreshold("   ", UnlockableFeature.Quests));
        ex.Message.ShouldContain("Threshold key");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NullThresholdKey()
    {
        Exceptions.DomainException ex = Should.Throw<Exceptions.DomainException>(
            () => new EngagementThreshold(null!, UnlockableFeature.Quests));
        ex.Message.ShouldContain("Threshold key");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SupportEquality_When_SameValues()
    {
        EngagementThreshold a = new("5 tasks created", UnlockableFeature.Quests);
        EngagementThreshold b = new("5 tasks created", UnlockableFeature.Quests);
        a.ShouldBe(b);
    }
}
