using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for WeeklyReviewSummary value object.
/// Maps to: docs/features/reflection/weekly-review.feature
/// Rule: "Free-tier users get a streamlined review covering essential retrospection"
/// </summary>
public sealed class WeeklyReviewSummaryTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSummary_When_ValidMetricsProvided()
    {
        // Given/When
        WeeklyReviewSummary summary = new(24, 30, 2, 11, new ExperiencePoints(420));

        // Then
        summary.TasksCompleted.ShouldBe(24);
        summary.TasksCreated.ShouldBe(30);
        summary.QuestsCompleted.ShouldBe(2);
        summary.CurrentStreak.ShouldBe(11);
        summary.XpEarned.Value.ShouldBe(420);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NegativeTasksCompleted()
    {
        Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(-1, 30, 2, 11, new ExperiencePoints(420)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NegativeTasksCreated()
    {
        Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(24, -1, 2, 11, new ExperiencePoints(420)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NegativeQuestsCompleted()
    {
        Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(24, 30, -1, 11, new ExperiencePoints(420)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NegativeCurrentStreak()
    {
        Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(24, 30, 2, -1, new ExperiencePoints(420)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_NullXpEarned()
    {
        Should.Throw<ArgumentNullException>(() =>
            new WeeklyReviewSummary(24, 30, 2, 11, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroValues_When_NoActivityInWeek()
    {
        WeeklyReviewSummary summary = new(0, 0, 0, 0, new ExperiencePoints(0));

        summary.TasksCompleted.ShouldBe(0);
        summary.TasksCreated.ShouldBe(0);
        summary.QuestsCompleted.ShouldBe(0);
        summary.CurrentStreak.ShouldBe(0);
        summary.XpEarned.Value.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveValueEquality_When_SameMetrics()
    {
        WeeklyReviewSummary a = new(24, 30, 2, 11, new ExperiencePoints(420));
        WeeklyReviewSummary b = new(24, 30, 2, 11, new ExperiencePoints(420));

        a.ShouldBe(b);
    }

    // ── Mutation-killing: DomainException message verification ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NegativeTasksCompleted()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(-1, 30, 2, 11, new ExperiencePoints(420)));
        ex.Message.ShouldContain("Tasks completed cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NegativeTasksCreated()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(24, -1, 2, 11, new ExperiencePoints(420)));
        ex.Message.ShouldContain("Tasks created cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NegativeQuestsCompleted()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(24, 30, -1, 11, new ExperiencePoints(420)));
        ex.Message.ShouldContain("Quests completed cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NegativeCurrentStreak()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new WeeklyReviewSummary(24, 30, 2, -1, new ExperiencePoints(420)));
        ex.Message.ShouldContain("Current streak cannot be negative");
    }
}
