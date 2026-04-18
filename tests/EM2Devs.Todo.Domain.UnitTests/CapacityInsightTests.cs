using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for CapacityInsight trend detection: capacity insight informs planning.
/// </summary>
public sealed class CapacityInsightTests
{
    private static List<int> List(params int[] values) => new List<int>(values);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportRising_When_RecentAverageExceedsPreviousByMoreThanBand()
    {
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(8, 8, 8),
            previous: List(4, 4, 4));
        insight.Trend.ShouldBe(CapacityTrend.Rising);
        insight.RecentAverage.ShouldBe(8.0);
        insight.PreviousAverage.ShouldBe(4.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportFalling_When_RecentAverageBelowPreviousByMoreThanBand()
    {
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(4, 4),
            previous: List(8, 8));
        insight.Trend.ShouldBe(CapacityTrend.Falling);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportStable_When_RecentAverageCloseToPrevious()
    {
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(6, 6),
            previous: List(6, 6));
        insight.Trend.ShouldBe(CapacityTrend.Stable);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportStable_When_DifferenceBelowStableBand()
    {
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(6, 6, 7),
            previous: List(6, 6, 6));
        insight.Trend.ShouldBe(CapacityTrend.Stable);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_RecentNull()
    {
        Should.Throw<ArgumentNullException>(() => CapacityInsight.Evaluate(null!, List(1)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_PreviousNull()
    {
        Should.Throw<ArgumentNullException>(() => CapacityInsight.Evaluate(List(1), null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_EitherListEmpty()
    {
        DomainException ex1 = Should.Throw<DomainException>(() => CapacityInsight.Evaluate(new List<int>(), List(1)));
        ex1.Message.ShouldContain("must contain data");
        DomainException ex2 = Should.Throw<DomainException>(() => CapacityInsight.Evaluate(List(1), new List<int>()));
        ex2.Message.ShouldContain("must contain data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportFalling_When_DeltaBelowNegativeBand()
    {
        // Kills the negative boundary mutation
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(6),
            previous: List(7));
        insight.Trend.ShouldBe(CapacityTrend.Falling);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportFalling_When_DeltaExactlyAtNegativeBand()
    {
        // Kills `delta <= -StableBand` -> `delta < -StableBand`.
        // delta must equal exactly -0.5.
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(5, 5),
            previous: List(5, 6));
        insight.Trend.ShouldBe(CapacityTrend.Falling);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportStable_When_DeltaInsideNegativeBand()
    {
        // delta = -0.3 (within stable band)
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(6, 6, 6),
            previous: List(6, 6, 7));  // avg 6.33 -> delta = -0.33
        insight.Trend.ShouldBe(CapacityTrend.Stable);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportRising_When_DifferenceExactlyAtBandThreshold()
    {
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(6, 7),
            previous: List(6));
        insight.Trend.ShouldBe(CapacityTrend.Rising);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RoundAverages()
    {
        CapacityInsight insight = CapacityInsight.Evaluate(
            recent: List(7, 8, 9),
            previous: List(5, 6, 7));
        insight.RecentAverage.ShouldBe(8.0);
        insight.PreviousAverage.ShouldBe(6.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnPlanningRecommendation_When_CapacityVariesAcrossDays()
    {
        var overview = WeeklyCapacityOverview.From(new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 8 }, { DayOfWeek.Tuesday, 6 }, { DayOfWeek.Wednesday, 6 },
            { DayOfWeek.Thursday, 5 }, { DayOfWeek.Friday, 4 }, { DayOfWeek.Saturday, 2 },
            { DayOfWeek.Sunday, 3 }
        });

        CapacityInsight insight = CapacityInsight.Evaluate(List(6, 7), List(5, 6));

        string? recommendation = CapacityInsight.GetPlanningRecommendation(overview);
        recommendation.ShouldNotBeNull();
        recommendation.ShouldContain("Monday");
        recommendation.ShouldContain("Saturday");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_CapacityIsUniformAcrossDays()
    {
        var overview = WeeklyCapacityOverview.From(new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 5 }, { DayOfWeek.Tuesday, 5 }, { DayOfWeek.Wednesday, 5 },
            { DayOfWeek.Thursday, 5 }, { DayOfWeek.Friday, 5 }, { DayOfWeek.Saturday, 5 },
            { DayOfWeek.Sunday, 5 }
        });

        CapacityInsight insight = CapacityInsight.Evaluate(List(5), List(5));

        CapacityInsight.GetPlanningRecommendation(overview).ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNull_When_OverviewIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            CapacityInsight.GetPlanningRecommendation(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnRecommendation_When_DifferenceIsExactlyTwo()
    {
        var overview = WeeklyCapacityOverview.From(new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 }, { DayOfWeek.Tuesday, 5 }, { DayOfWeek.Wednesday, 5 },
            { DayOfWeek.Thursday, 5 }, { DayOfWeek.Friday, 4 }, { DayOfWeek.Saturday, 4 },
            { DayOfWeek.Sunday, 4 }
        });

        string? recommendation = CapacityInsight.GetPlanningRecommendation(overview);
        recommendation.ShouldNotBeNull();
    }
}
