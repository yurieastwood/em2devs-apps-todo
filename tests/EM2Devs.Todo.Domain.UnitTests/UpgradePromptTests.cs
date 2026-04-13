using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the UpgradePrompt value object.
/// Maps to: docs/features/monetisation/subscription-tiers.feature
/// Scenario: "Free-tier user encounters a premium feature"
/// </summary>
public sealed class UpgradePromptTests
{
    private static readonly DateTimeOffset _now = new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateUpgradePrompt_When_ValidParameters()
    {
        var prompt = new UpgradePrompt("Sagas", _now);

        prompt.FeatureName.ShouldBe("Sagas");
        prompt.LastShownAt.ShouldBe(_now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FeatureNameIsEmpty()
    {
        var ex = Should.Throw<DomainException>(() => new UpgradePrompt("", _now));
        ex.Message.ShouldContain("Feature name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FeatureNameIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(() => new UpgradePrompt("  ", _now));
        ex.Message.ShouldContain("Feature name cannot be empty");
    }

    // --- Scenario: Same feature prompt should not appear more than once per week ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotShowAgain_When_LessThanOneWeek()
    {
        var prompt = new UpgradePrompt("Sagas", _now);

        prompt.CanShowAgain(_now.AddDays(6)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowAgain_When_ExactlyOneWeek()
    {
        var prompt = new UpgradePrompt("Sagas", _now);

        prompt.CanShowAgain(_now.AddDays(7)).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowAgain_When_MoreThanOneWeek()
    {
        var prompt = new UpgradePrompt("Sagas", _now);

        prompt.CanShowAgain(_now.AddDays(8)).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotShowAgain_When_SameTime()
    {
        var prompt = new UpgradePrompt("Sagas", _now);

        prompt.CanShowAgain(_now).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordShown_When_PromptDisplayed()
    {
        var prompt = new UpgradePrompt("Sagas", _now);
        var newTime = _now.AddDays(8);

        var updated = prompt.RecordShown(newTime);

        updated.FeatureName.ShouldBe("Sagas");
        updated.LastShownAt.ShouldBe(newTime);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveCorrectMinimumInterval()
    {
        UpgradePrompt.MinimumInterval.ShouldBe(TimeSpan.FromDays(7));
    }
}
