using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for EstimationRecord entity.
/// Tests encode behaviors from time-estimation.feature (ADR-0003).
/// </summary>
public sealed class EstimationRecordTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculatePositiveVariance_When_ActualExceedsEstimate()
    {
        // Given — estimated 1 hour, actual 1h40m
        TimeEstimate estimated = TimeEstimate.FromMinutes(60);
        TimeEstimate actual = TimeEstimate.FromMinutes(100);

        // When
        EstimationRecord record = EstimationRecord.Create(estimated, actual);

        // Then — (100-60)/60 = 66.7%
        record.VariancePercent.ShouldBe(66.7, tolerance: 0.1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateNegativeVariance_When_ActualLessThanEstimate()
    {
        // Given — estimated 2 hours, actual 30 minutes
        TimeEstimate estimated = TimeEstimate.FromMinutes(120);
        TimeEstimate actual = TimeEstimate.FromMinutes(30);

        // When
        EstimationRecord record = EstimationRecord.Create(estimated, actual);

        // Then — (30-120)/120 = -75%
        record.VariancePercent.ShouldBe(-75.0, tolerance: 0.1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateZeroVariance_When_ActualEqualsEstimate()
    {
        // Given
        TimeEstimate estimated = TimeEstimate.FromMinutes(60);
        TimeEstimate actual = TimeEstimate.FromMinutes(60);

        // When
        EstimationRecord record = EstimationRecord.Create(estimated, actual);

        // Then
        record.VariancePercent.ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveEstimateAndActual_When_RecordCreated()
    {
        // Given
        TimeEstimate estimated = TimeEstimate.FromMinutes(30);
        TimeEstimate actual = TimeEstimate.FromMinutes(45);

        // When
        EstimationRecord record = EstimationRecord.Create(estimated, actual);

        // Then
        record.Id.Value.ShouldNotBe(Guid.Empty);
        record.Estimated.Minutes.ShouldBe(30);
        record.Actual.Minutes.ShouldBe(45);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MinutesIsNegative()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => TimeEstimate.FromMinutes(-1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_MinutesIsZero()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => TimeEstimate.FromMinutes(0));
        ex.Message.ShouldContain("must be greater than zero");
    }
}
