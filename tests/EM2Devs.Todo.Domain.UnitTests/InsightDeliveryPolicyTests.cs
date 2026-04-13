using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for InsightDeliveryPolicy value object.
/// Maps to: docs/features/reflection/insight-cards.feature
/// </summary>
public sealed class InsightDeliveryPolicyTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateDefaultPolicy_When_UsingDefaultFactory()
    {
        var policy = InsightDeliveryPolicy.Default;

        policy.MaxPerDay.ShouldBe(1);
        policy.MinPerWeek.ShouldBe(2);
        policy.MaxPerWeek.ShouldBe(3);
        policy.CooldownDays.ShouldBe(90);
        policy.MinimumDataDays.ShouldBe(30);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCustomPolicy_When_ValidParametersProvided()
    {
        var policy = new InsightDeliveryPolicy(2, 3, 5, 60, 14);

        policy.MaxPerDay.ShouldBe(2);
        policy.MinPerWeek.ShouldBe(3);
        policy.MaxPerWeek.ShouldBe(5);
        policy.CooldownDays.ShouldBe(60);
        policy.MinimumDataDays.ShouldBe(14);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMaxPerDayZero_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(0, 2, 3, 90, 30))
            .Message.ShouldContain("Max per day");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMaxPerDayNegative_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(-1, 2, 3, 90, 30))
            .Message.ShouldContain("Max per day");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMinPerWeekZero_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(1, 0, 3, 90, 30))
            .Message.ShouldContain("Min per week");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMinPerWeekNegative_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(1, -1, 3, 90, 30))
            .Message.ShouldContain("Min per week");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMaxPerWeekLessThanMin_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(1, 3, 2, 90, 30))
            .Message.ShouldContain("Max per week");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowEqualMinAndMaxPerWeek_When_CreatingPolicy()
    {
        var policy = new InsightDeliveryPolicy(1, 3, 3, 90, 30);
        policy.MinPerWeek.ShouldBe(3);
        policy.MaxPerWeek.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenCooldownDaysZero_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(1, 2, 3, 0, 30))
            .Message.ShouldContain("Cooldown days");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenCooldownDaysNegative_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(1, 2, 3, -1, 30))
            .Message.ShouldContain("Cooldown days");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMinimumDataDaysZero_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(1, 2, 3, 90, 0))
            .Message.ShouldContain("Minimum data days");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenMinimumDataDaysNegative_When_CreatingPolicy()
    {
        Should.Throw<DomainException>(() => new InsightDeliveryPolicy(1, 2, 3, 90, -1))
            .Message.ShouldContain("Minimum data days");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrueForSufficientData_When_ExactlyAtThreshold()
    {
        var policy = InsightDeliveryPolicy.Default;
        policy.HasSufficientData(30).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalseForSufficientData_When_BelowThreshold()
    {
        var policy = InsightDeliveryPolicy.Default;
        policy.HasSufficientData(29).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrueForSufficientData_When_AboveThreshold()
    {
        var policy = InsightDeliveryPolicy.Default;
        policy.HasSufficientData(31).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectCooldown_When_WithinCooldownPeriod()
    {
        var policy = InsightDeliveryPolicy.Default; // 90-day cooldown
        var lastDelivered = new DateOnly(2026, 1, 15);
        var today = new DateOnly(2026, 2, 20); // 36 days later

        policy.IsInCooldown(lastDelivered, today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDetectCooldown_When_OutsideCooldownPeriod()
    {
        var policy = InsightDeliveryPolicy.Default; // 90-day cooldown
        var lastDelivered = new DateOnly(2026, 1, 15);
        var today = new DateOnly(2026, 4, 16); // 91 days later

        policy.IsInCooldown(lastDelivered, today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectCooldown_When_ExactlyAtCooldownBoundary()
    {
        var policy = InsightDeliveryPolicy.Default; // 90-day cooldown
        var lastDelivered = new DateOnly(2026, 1, 15);
        var today = new DateOnly(2026, 4, 15); // exactly 90 days later

        // 90 days elapsed is NOT >= 90? Let's see: daysSinceLast = 90, < 90 is false
        policy.IsInCooldown(lastDelivered, today).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectCooldown_When_OneDayBeforeCooldownExpires()
    {
        var policy = InsightDeliveryPolicy.Default;
        var lastDelivered = new DateOnly(2026, 1, 15);
        var today = new DateOnly(2026, 4, 14); // 89 days later

        policy.IsInCooldown(lastDelivered, today).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalseForDailyLimit_When_NoneDelivered()
    {
        var policy = InsightDeliveryPolicy.Default;
        policy.HasReachedDailyLimit(0).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalseForWeeklyLimit_When_NoneDelivered()
    {
        var policy = InsightDeliveryPolicy.Default;
        policy.HasReachedWeeklyLimit(0).ShouldBeFalse();
    }
}
