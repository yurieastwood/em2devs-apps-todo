using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for WeeklyCapacityOverview: view weekly capacity overview.
/// </summary>
public sealed class WeeklyCapacityOverviewTests
{
    private static Dictionary<DayOfWeek, int> FullWeek(
        int mon = 6, int tue = 6, int wed = 6, int thu = 6, int fri = 4, int sat = 2, int sun = 3)
    {
        return new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, mon },
            { DayOfWeek.Tuesday, tue },
            { DayOfWeek.Wednesday, wed },
            { DayOfWeek.Thursday, thu },
            { DayOfWeek.Friday, fri },
            { DayOfWeek.Saturday, sat },
            { DayOfWeek.Sunday, sun },
        };
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeCapacityPerDayOfWeek()
    {
        WeeklyCapacityOverview overview = WeeklyCapacityOverview.From(FullWeek());
        overview.CapacityByDay[DayOfWeek.Monday].ShouldBe(6);
        overview.CapacityByDay[DayOfWeek.Saturday].ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IdentifyMostAndLeastProductiveDays()
    {
        WeeklyCapacityOverview overview = WeeklyCapacityOverview.From(
            FullWeek(mon: 8, fri: 4, sat: 2));
        overview.MostProductiveDay.ShouldBe(DayOfWeek.Monday);
        overview.LeastProductiveDay.ShouldBe(DayOfWeek.Saturday);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeAverageDailyCapacity()
    {
        WeeklyCapacityOverview overview = WeeklyCapacityOverview.From(FullWeek());
        // 6+6+6+6+4+2+3 = 33; 33 / 7 = 4 integer division
        overview.AverageDailyCapacity.ShouldBe(4);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_NotAllDaysProvided()
    {
        Dictionary<DayOfWeek, int> partial = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Monday, 6 },
        };
        DomainException ex = Should.Throw<DomainException>(() => WeeklyCapacityOverview.From(partial));
        ex.Message.ShouldContain("all 7 days");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_CapacityNegative()
    {
        Dictionary<DayOfWeek, int> week = FullWeek(mon: -1);
        DomainException ex = Should.Throw<DomainException>(() => WeeklyCapacityOverview.From(week));
        ex.Message.ShouldContain("Capacity cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptZeroCapacityForDay()
    {
        // Kills `kvp.Value < 0` -> `kvp.Value <= 0` mutation.
        Dictionary<DayOfWeek, int> week = FullWeek(mon: 0);
        WeeklyCapacityOverview overview = WeeklyCapacityOverview.From(week);
        overview.CapacityByDay[DayOfWeek.Monday].ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_Null()
    {
        Should.Throw<ArgumentNullException>(() => WeeklyCapacityOverview.From(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SelectEarlierDay_When_TiedCapacity()
    {
        // All days equal -> tie-break by day order ascending for both
        Dictionary<DayOfWeek, int> week = new Dictionary<DayOfWeek, int>
        {
            { DayOfWeek.Sunday, 5 },
            { DayOfWeek.Monday, 5 },
            { DayOfWeek.Tuesday, 5 },
            { DayOfWeek.Wednesday, 5 },
            { DayOfWeek.Thursday, 5 },
            { DayOfWeek.Friday, 5 },
            { DayOfWeek.Saturday, 5 },
        };

        WeeklyCapacityOverview overview = WeeklyCapacityOverview.From(week);
        // DayOfWeek enum: Sunday=0, Monday=1,..., Saturday=6.
        // Most productive: highest value then earliest DayOfWeek -> Sunday (0)
        overview.MostProductiveDay.ShouldBe(DayOfWeek.Sunday);
        overview.LeastProductiveDay.ShouldBe(DayOfWeek.Sunday);
    }
}
