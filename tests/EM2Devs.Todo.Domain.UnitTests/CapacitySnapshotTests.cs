using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for CapacitySnapshot entity.
/// Tests encode behaviors from capacity-modelling.feature (ADR-0003).
/// </summary>
public sealed class CapacitySnapshotTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithCapacity_When_WeekdaySnapshotCreated()
    {
        // Given
        DailyCapacity capacity = DailyCapacity.FromTaskUnits(6);

        // When
        CapacitySnapshot snapshot = CapacitySnapshot.Create(capacity, DayOfWeek.Monday);

        // Then
        snapshot.Id.Value.ShouldNotBe(Guid.Empty);
        snapshot.Capacity.TaskUnits.ShouldBe(6);
        snapshot.DayOfWeek.ShouldBe(DayOfWeek.Monday);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DetectOvercommitment_When_ScheduledExceedsCapacity()
    {
        // Given
        CapacitySnapshot snapshot = CapacitySnapshot.Create(
            DailyCapacity.FromTaskUnits(6), DayOfWeek.Wednesday);

        // When
        bool overcommitted = snapshot.IsOvercommitted(scheduledUnits: 10);

        // Then
        overcommitted.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDetectOvercommitment_When_ScheduledWithinCapacity()
    {
        // Given
        CapacitySnapshot snapshot = CapacitySnapshot.Create(
            DailyCapacity.FromTaskUnits(6), DayOfWeek.Wednesday);

        // When
        bool overcommitted = snapshot.IsOvercommitted(scheduledUnits: 4);

        // Then
        overcommitted.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDetectOvercommitment_When_ScheduledEqualsCapacity()
    {
        // Given
        CapacitySnapshot snapshot = CapacitySnapshot.Create(
            DailyCapacity.FromTaskUnits(6), DayOfWeek.Wednesday);

        // When
        bool overcommitted = snapshot.IsOvercommitted(scheduledUnits: 6);

        // Then
        overcommitted.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CapacityIsNegative()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => DailyCapacity.FromTaskUnits(-1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroCapacity_When_RestDayConfigured()
    {
        // Given / When
        DailyCapacity capacity = DailyCapacity.FromTaskUnits(0);

        // Then
        capacity.TaskUnits.ShouldBe(0);
    }
}
