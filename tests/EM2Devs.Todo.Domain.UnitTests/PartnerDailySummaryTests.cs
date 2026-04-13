using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for PartnerDailySummary value object.
/// Maps to: docs/features/social/accountability-partners.feature
/// Scenario: "View partner's daily summary"
/// Scenario: "Partner sees my summary"
/// Scenario: "View partner's streak without revealing task details"
/// </summary>
public sealed class PartnerDailySummaryTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSummary_When_ValidInputProvided()
    {
        // Given / When
        var summary = new PartnerDailySummary(5, 12, 150, 2, _today);

        // Then
        summary.TasksCompleted.ShouldBe(5);
        summary.CurrentStreak.ShouldBe(12);
        summary.XpEarnedToday.ShouldBe(150);
        summary.ActiveQuestCount.ShouldBe(2);
        summary.Date.ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroValues_When_NoActivityToday()
    {
        var summary = new PartnerDailySummary(0, 0, 0, 0, _today);
        summary.TasksCompleted.ShouldBe(0);
        summary.CurrentStreak.ShouldBe(0);
        summary.XpEarnedToday.ShouldBe(0);
        summary.ActiveQuestCount.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TasksCompletedIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new PartnerDailySummary(-1, 0, 0, 0, _today));
        ex.Message.ShouldContain("Tasks completed cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CurrentStreakIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new PartnerDailySummary(0, -1, 0, 0, _today));
        ex.Message.ShouldContain("Current streak cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_XpEarnedTodayIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new PartnerDailySummary(0, 0, -1, 0, _today));
        ex.Message.ShouldContain("XP earned today cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ActiveQuestCountIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new PartnerDailySummary(0, 0, 0, -1, _today));
        ex.Message.ShouldContain("Active quest count cannot be negative");
    }
}
