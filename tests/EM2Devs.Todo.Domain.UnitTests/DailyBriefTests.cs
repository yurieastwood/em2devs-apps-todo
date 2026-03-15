using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for DailyBrief entity.
/// Tests encode behaviors from daily-brief.feature (ADR-0003).
/// </summary>
public sealed class DailyBriefTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithGeneratedStatus_When_BriefCreated()
    {
        // Given
        DateOnly today = new(2026, 3, 15);
        List<TaskId> recommended = [TaskId.New(), TaskId.New(), TaskId.New()];

        // When
        DailyBrief brief = DailyBrief.Create(today, recommended);

        // Then
        brief.Id.Value.ShouldNotBe(Guid.Empty);
        brief.Date.ShouldBe(today);
        brief.RecommendedTaskIds.Count.ShouldBe(3);
        brief.Status.ShouldBe(DailyBriefStatus.Generated);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToAccepted_When_Accepted()
    {
        // Given
        DailyBrief brief = DailyBrief.Create(new DateOnly(2026, 3, 15), [TaskId.New()]);

        // When
        brief.Accept();

        // Then
        brief.Status.ShouldBe(DailyBriefStatus.Accepted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToDismissed_When_Dismissed()
    {
        // Given
        DailyBrief brief = DailyBrief.Create(new DateOnly(2026, 3, 15), [TaskId.New()]);

        // When
        brief.Dismiss();

        // Then
        brief.Status.ShouldBe(DailyBriefStatus.Dismissed);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToModified_When_Modified()
    {
        // Given
        DailyBrief brief = DailyBrief.Create(new DateOnly(2026, 3, 15), [TaskId.New(), TaskId.New()]);
        List<TaskId> newOrder = [TaskId.New(), TaskId.New(), TaskId.New()];

        // When
        brief.Modify(newOrder);

        // Then
        brief.Status.ShouldBe(DailyBriefStatus.Modified);
        brief.RecommendedTaskIds.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CreatedWithEmptyTaskList()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(new DateOnly(2026, 3, 15), []));
        ex.Message.ShouldContain("at least");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ModifiedWithEmptyTaskList()
    {
        // Given
        DailyBrief brief = DailyBrief.Create(new DateOnly(2026, 3, 15), [TaskId.New()]);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => brief.Modify([]));
        ex.Message.ShouldContain("at least");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatedWithNullTaskList()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(() =>
            DailyBrief.Create(new DateOnly(2026, 3, 15), null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ModifiedWithNullTaskList()
    {
        // Given
        DailyBrief brief = DailyBrief.Create(new DateOnly(2026, 3, 15), [TaskId.New()]);

        // When / Then
        Should.Throw<ArgumentNullException>(() => brief.Modify(null!));
    }
}
