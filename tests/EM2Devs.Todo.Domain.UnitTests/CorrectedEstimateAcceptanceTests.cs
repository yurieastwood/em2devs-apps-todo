using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for CorrectedEstimateAcceptance: user accepts corrected estimate but completes in original time.
/// Also covers dismissal tracking.
/// </summary>
public sealed class CorrectedEstimateAcceptanceTests
{
    private static TimeEstimate Minutes(int m) => TimeEstimate.FromMinutes(m);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordAcceptance_With_OriginalAndActualTimes()
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(120),
            acceptedEstimate: Minutes(168),
            actualTime: Minutes(120),
            wasAccepted: true);

        acceptance.OriginalEstimate.Minutes.ShouldBe(120);
        acceptance.AcceptedEstimate.Minutes.ShouldBe(168);
        acceptance.ActualTime.Minutes.ShouldBe(120);
        acceptance.WasAccepted.ShouldBeTrue();
        acceptance.OriginalWasMoreAccurate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IndicateAcceptedEstimateMoreAccurate_When_ActualCloserToAccepted()
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(60),
            acceptedEstimate: Minutes(90),
            actualTime: Minutes(85),
            wasAccepted: true);

        acceptance.OriginalWasMoreAccurate.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ProduceNegativeAdjustment_When_OriginalWasMoreAccurate()
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(120),
            acceptedEstimate: Minutes(168),
            actualTime: Minutes(120),
            wasAccepted: true);

        // Current bias was +40%, original was more accurate, so adjustment should reduce bias toward zero (negative number).
        double adjustment = acceptance.ComputeBiasAdjustment(40.0);
        adjustment.ShouldBe(-4.0); // 40 * 0.1 dampening
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ProduceZeroAdjustment_When_CorrectedEstimateWasMoreAccurate()
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(60),
            acceptedEstimate: Minutes(90),
            actualTime: Minutes(85),
            wasAccepted: true);

        acceptance.ComputeBiasAdjustment(40.0).ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ProduceZeroAdjustment_When_NotAccepted()
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(120),
            acceptedEstimate: Minutes(120),
            actualTime: Minutes(120),
            wasAccepted: false);

        acceptance.ComputeBiasAdjustment(40.0).ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_DismissedButAcceptedEstimateDiffersFromOriginal()
    {
        DomainException ex = Should.Throw<DomainException>(() => CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(60),
            acceptedEstimate: Minutes(90),
            actualTime: Minutes(60),
            wasAccepted: false));
        ex.Message.ShouldContain("accepted estimate must equal the original");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportOriginalNotMoreAccurate_When_BothEquidistant()
    {
        // Both original and accepted are equidistant from actual -> strictly less than fails.
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(80),
            acceptedEstimate: Minutes(120),
            actualTime: Minutes(100),
            wasAccepted: true);
        acceptance.OriginalWasMoreAccurate.ShouldBeFalse();
        acceptance.ComputeBiasAdjustment(40.0).ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDampening_WithSpecificMessage()
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(120),
            acceptedEstimate: Minutes(168),
            actualTime: Minutes(120),
            wasAccepted: true);
        DomainException ex = Should.Throw<DomainException>(() => acceptance.ComputeBiasAdjustment(40.0, 0));
        ex.Message.ShouldContain("dampening");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_AnyRequiredArgumentIsNull()
    {
        Should.Throw<ArgumentNullException>(() => CorrectedEstimateAcceptance.Create(null!, Minutes(1), Minutes(1), true));
        Should.Throw<ArgumentNullException>(() => CorrectedEstimateAcceptance.Create(Minutes(1), null!, Minutes(1), true));
        Should.Throw<ArgumentNullException>(() => CorrectedEstimateAcceptance.Create(Minutes(1), Minutes(1), null!, true));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.1)]
    [InlineData(1.5)]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_DampeningOutOfRange(double dampening)
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(120),
            acceptedEstimate: Minutes(168),
            actualTime: Minutes(120),
            wasAccepted: true);

        Should.Throw<DomainException>(() => acceptance.ComputeBiasAdjustment(40.0, dampening));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowDampeningOfOne()
    {
        CorrectedEstimateAcceptance acceptance = CorrectedEstimateAcceptance.Create(
            originalEstimate: Minutes(120),
            acceptedEstimate: Minutes(168),
            actualTime: Minutes(120),
            wasAccepted: true);

        acceptance.ComputeBiasAdjustment(40.0, 1.0).ShouldBe(-40.0);
    }
}
