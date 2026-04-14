using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for EstimationDashboard: view estimation accuracy dashboard.
/// </summary>
public sealed class EstimationDashboardTests
{
    private static TimeEstimate Minutes(int m) => TimeEstimate.FromMinutes(m);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeOverallAccuracy_FromRecords()
    {
        // Two records, variance +10 and -10 => avg abs variance = 10 => accuracy 90%
        EstimationRecord r1 = EstimationRecord.Create(Minutes(100), Minutes(110), TaskCategory.From("writing"));
        EstimationRecord r2 = EstimationRecord.Create(Minutes(100), Minutes(90), TaskCategory.From("writing"));

        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { r1, r2 },
            new List<double> { 80.0, 90.0 });

        dashboard.OverallAccuracyPercent.ShouldBe(90.0);
        dashboard.AccuracyTrend.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BreakdownAccuracy_ByCategory()
    {
        EstimationRecord writing1 = EstimationRecord.Create(Minutes(100), Minutes(140), TaskCategory.From("writing"));
        EstimationRecord writing2 = EstimationRecord.Create(Minutes(100), Minutes(140), TaskCategory.From("writing"));
        EstimationRecord review1 = EstimationRecord.Create(Minutes(100), Minutes(70), TaskCategory.From("review"));

        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { writing1, writing2, review1 },
            new List<double> { 50.0, 60.0 });

        dashboard.PerCategory.Count.ShouldBe(2);
        CategoryAccuracyStats writingStats = dashboard.PerCategory.First(s => s.Category.Value == "writing");
        writingStats.RecordCount.ShouldBe(2);
        writingStats.AverageVariancePercent.ShouldBe(40.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeTrendLine()
    {
        EstimationRecord record = EstimationRecord.Create(Minutes(60), Minutes(60), TaskCategory.From("writing"));
        List<double> trend = new List<double> { 50.0, 70.0, 80.0 };

        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { record },
            trend);
        dashboard.AccuracyTrend.ShouldBe(trend);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_RecordsNull()
    {
        Should.Throw<ArgumentNullException>(() => EstimationDashboard.Build(null!, new List<double>()));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_TrendNull()
    {
        EstimationRecord record = EstimationRecord.Create(Minutes(60), Minutes(60), TaskCategory.From("writing"));
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => EstimationDashboard.Build(new List<EstimationRecord> { record }, null!));
        ex.ParamName.ShouldBe("accuracyTrend");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_RecordsEmpty()
    {
        Should.Throw<DomainException>(() => EstimationDashboard.Build(new List<EstimationRecord>(), new List<double>()));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeNullCategoryRecords_FromPerCategoryBreakdown()
    {
        EstimationRecord withCat = EstimationRecord.Create(Minutes(100), Minutes(100), TaskCategory.From("writing"));
        EstimationRecord noCat = EstimationRecord.Create(Minutes(100), Minutes(100));

        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { withCat, noCat },
            new List<double> { 100.0 });

        dashboard.PerCategory.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowCategoryStats_When_InvalidRecordCount()
    {
        DomainException ex = Should.Throw<DomainException>(() => new CategoryAccuracyStats(TaskCategory.From("x"), 0, 0.0, 100.0));
        ex.Message.ShouldContain("Record count");
        DomainException exNeg = Should.Throw<DomainException>(() => new CategoryAccuracyStats(TaskCategory.From("x"), -1, 0.0, 100.0));
        exNeg.Message.ShouldContain("Record count");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDashboardBuild_WithEmptyRecordsMessage()
    {
        DomainException ex = Should.Throw<DomainException>(() => EstimationDashboard.Build(new List<EstimationRecord>(), new List<double>()));
        ex.Message.ShouldContain("At least one");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeAverageNotMin_For_OverallAccuracy()
    {
        // Kills Average() -> Min() mutation. Two records with abs variances 10 and 30 -> avg=20 -> accuracy 80.
        // Under Min: min=10 -> accuracy 90. Assert we see 80 (not 90).
        EstimationRecord r1 = EstimationRecord.Create(Minutes(100), Minutes(110), TaskCategory.From("x")); // +10%
        EstimationRecord r2 = EstimationRecord.Create(Minutes(100), Minutes(130), TaskCategory.From("x")); // +30%
        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { r1, r2 },
            new List<double> { 80.0 });
        dashboard.OverallAccuracyPercent.ShouldBe(80.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeAverageNotMin_For_PerCategory()
    {
        // Per-category: two records with variances 10 and 30 -> avg = 20.
        // Under Min mutation would be 10. Assert average.
        EstimationRecord r1 = EstimationRecord.Create(Minutes(100), Minutes(110), TaskCategory.From("writing"));
        EstimationRecord r2 = EstimationRecord.Create(Minutes(100), Minutes(130), TaskCategory.From("writing"));
        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { r1, r2 },
            new List<double> { 50.0 });
        CategoryAccuracyStats stats = dashboard.PerCategory.Single();
        stats.AverageVariancePercent.ShouldBe(20.0);
        stats.AccuracyPercent.ShouldBe(80.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeSubtraction_NotAddition_ForAccuracy()
    {
        // Kills `100 - Math.Abs(x)` -> `100 + Math.Abs(x)`.
        EstimationRecord r = EstimationRecord.Create(Minutes(100), Minutes(120), TaskCategory.From("x")); // +20%
        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { r },
            new List<double> { 80.0 });
        CategoryAccuracyStats stats = dashboard.PerCategory.Single();
        stats.AccuracyPercent.ShouldBe(80.0); // NOT 120
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseMathAbsToComputeCategoryAccuracy_EvenForNegativeVariance()
    {
        // Kills Max() -> Min() mutation on `Math.Max(0, 100 - Math.Abs(avgVariance))`.
        // avgVariance negative becomes positive by abs.
        EstimationRecord r = EstimationRecord.Create(Minutes(100), Minutes(80), TaskCategory.From("x")); // -20%
        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { r },
            new List<double> { 80.0 });
        CategoryAccuracyStats stats = dashboard.PerCategory.Single();
        stats.AccuracyPercent.ShouldBe(80.0);
        stats.AverageVariancePercent.ShouldBe(-20.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowCategoryStats_When_CategoryNull()
    {
        Should.Throw<ArgumentNullException>(() => new CategoryAccuracyStats(null!, 1, 0.0, 100.0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FloorAccuracyAtZero_When_VarianceExceeds100Percent()
    {
        EstimationRecord huge = EstimationRecord.Create(Minutes(10), Minutes(100)); // +900% variance
        EstimationDashboard dashboard = EstimationDashboard.Build(
            new List<EstimationRecord> { huge },
            new List<double> { 0.0 });
        dashboard.OverallAccuracyPercent.ShouldBe(0.0);
    }
}
