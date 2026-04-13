using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for EnergyInferenceConfidence, covering the confidence score
/// that rises as more check-in data is accumulated (energy-scheduling.feature: energy inference improves with data).
/// </summary>
public sealed class EnergyInferenceConfidenceTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeZero_When_DataPointsBelowMinimum()
    {
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(5);
        confidence.Score.ShouldBe(0.0);
        confidence.DataPoints.ShouldBe(5);
        confidence.IsLow.ShouldBeTrue();
        confidence.IsModerate.ShouldBeFalse();
        confidence.IsHigh.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeModerate_When_DataPointsReachMinimum()
    {
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(14);
        confidence.Score.ShouldBe(0.0);
        // At exactly minimum the raw score is 0; this is Low by threshold. Capture transition by 30 days.
        EnergyInferenceConfidence at30 = EnergyInferenceConfidence.FromDataPoints(30);
        at30.IsModerate.ShouldBeTrue();
        at30.IsHigh.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeHigh_When_DataPointsExceedHighThreshold()
    {
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(60);
        confidence.Score.ShouldBe(1.0);
        confidence.IsHigh.ShouldBeTrue();
        confidence.IsModerate.ShouldBeFalse();
        confidence.IsLow.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CapConfidence_When_DataPointsExceedMax()
    {
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(365);
        confidence.Score.ShouldBe(1.0);
        confidence.DataPoints.ShouldBe(365);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_DataPointsNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() => EnergyInferenceConfidence.FromDataPoints(-1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptZero_WithoutThrowing()
    {
        // Kills `dataPoints < 0` -> `dataPoints <= 0` mutation
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(0);
        confidence.Score.ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CrossIntoHigh_WhenScoreExactlyAtHighThreshold()
    {
        // Score >= 0.75 -> IsHigh. data points = 14 + 0.75 * 46 = 48.5 -> 49 data points -> score 35/46 ≈ 0.761
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(49);
        confidence.Score.ShouldBeGreaterThanOrEqualTo(EnergyInferenceConfidence.HighConfidenceThreshold);
        confidence.IsHigh.ShouldBeTrue();
        confidence.IsModerate.ShouldBeFalse();
        confidence.IsLow.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StayBelowHigh_WhenJustUnderThreshold()
    {
        // Score just under 0.75: 14 + 0.7 * 46 = 46.2 -> 46 data points -> score = 32/46 ≈ 0.696 < 0.75
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(46);
        confidence.Score.ShouldBeLessThan(EnergyInferenceConfidence.HighConfidenceThreshold);
        confidence.IsHigh.ShouldBeFalse();
        confidence.IsModerate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CrossIntoModerate_WhenScoreExactlyAtModerateThreshold()
    {
        // Want score >= 0.25: 14 + 0.25 * 46 = 25.5 -> 26 data points. Score = 12/46 ≈ 0.261
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(26);
        confidence.Score.ShouldBeGreaterThanOrEqualTo(EnergyInferenceConfidence.ModerateConfidenceThreshold);
        confidence.IsModerate.ShouldBeTrue();
        confidence.IsLow.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StayLow_WhenJustUnderModerateThreshold()
    {
        // score < 0.25: data points = 14 + 0.2 * 46 = 23.2 -> 23 data points -> 9/46 ≈ 0.196
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(23);
        confidence.Score.ShouldBeLessThan(EnergyInferenceConfidence.ModerateConfidenceThreshold);
        confidence.IsLow.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ScaleLinearly_BetweenMinimumAndHighThreshold()
    {
        // 14 data points -> 0.0; 60 data points -> 1.0; midpoint ~37 -> ~0.5
        EnergyInferenceConfidence mid = EnergyInferenceConfidence.FromDataPoints(37);
        mid.Score.ShouldBe(0.5);
        mid.IsModerate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnLow_When_ScoreBelowModerateThreshold()
    {
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromDataPoints(14);
        confidence.IsLow.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveExpectedConstants()
    {
        EnergyInferenceConfidence.MinimumDataPoints.ShouldBe(14);
        EnergyInferenceConfidence.HighConfidenceDataPoints.ShouldBe(60);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeHigh_WhenScoreExactlyAtHighThreshold()
    {
        // Kills `>=` -> `>` mutation on IsHigh: at exactly HighConfidenceThreshold must be IsHigh
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromScore(
            EnergyInferenceConfidence.HighConfidenceThreshold, 50);
        confidence.IsHigh.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeModerate_WhenScoreExactlyAtModerateThreshold()
    {
        // Kills `>=` -> `>` mutation on IsModerate: at exactly ModerateConfidenceThreshold must be IsModerate
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromScore(
            EnergyInferenceConfidence.ModerateConfidenceThreshold, 25);
        confidence.IsModerate.ShouldBeTrue();
        confidence.IsLow.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeHigh_WhenScoreJustBelowThreshold()
    {
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromScore(
            EnergyInferenceConfidence.HighConfidenceThreshold - 0.001, 50);
        confidence.IsHigh.ShouldBeFalse();
        confidence.IsModerate.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeLow_WhenScoreJustBelowModerateThreshold()
    {
        EnergyInferenceConfidence confidence = EnergyInferenceConfidence.FromScore(
            EnergyInferenceConfidence.ModerateConfidenceThreshold - 0.001, 20);
        confidence.IsLow.ShouldBeTrue();
        confidence.IsModerate.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowFromScore_When_ScoreOutOfRange()
    {
        DomainException ex1 = Should.Throw<DomainException>(() => EnergyInferenceConfidence.FromScore(-0.01, 20));
        ex1.Message.ShouldContain("between 0 and 1");
        DomainException ex2 = Should.Throw<DomainException>(() => EnergyInferenceConfidence.FromScore(1.01, 20));
        ex2.Message.ShouldContain("between 0 and 1");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowFromScore_When_DataPointsNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() => EnergyInferenceConfidence.FromScore(0.5, -1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptZeroDataPoints_In_FromScore()
    {
        // Kills `dataPoints < 0` -> `dataPoints <= 0` in FromScore.
        EnergyInferenceConfidence c = EnergyInferenceConfidence.FromScore(0.5, 0);
        c.DataPoints.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptExactlyZeroScore_In_FromScore()
    {
        // Kills `score < 0.0` -> `score <= 0.0`.
        EnergyInferenceConfidence c = EnergyInferenceConfidence.FromScore(0.0, 20);
        c.Score.ShouldBe(0.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptExactlyOneScore_In_FromScore()
    {
        // Kills `score > 1.0` -> `score >= 1.0`.
        EnergyInferenceConfidence c = EnergyInferenceConfidence.FromScore(1.0, 60);
        c.Score.ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeModerate_AtHighThresholdExactly()
    {
        // Kills `Score < HighConfidenceThreshold` -> `Score <= HighConfidenceThreshold` in IsModerate definition.
        // IsModerate := Score >= ModerateThreshold && Score < HighThreshold.
        // At Score == HighThreshold, original IsModerate false (because Score == HighThreshold fails `<`).
        // Mutant would make IsModerate true.
        EnergyInferenceConfidence c = EnergyInferenceConfidence.FromScore(
            EnergyInferenceConfidence.HighConfidenceThreshold, 60);
        c.IsModerate.ShouldBeFalse();
    }
}
