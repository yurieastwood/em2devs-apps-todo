using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for EnergyPatternDetector, covering time-of-day and weekly pattern inference.
/// Based on energy-scheduling.feature: infer energy from time-of-day patterns, pattern across weeks.
/// </summary>
public sealed class EnergyPatternDetectorTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_InferHighEnergy_When_CurrentHourMatchesPeakProductivityWindow()
    {
        // Given — user completes hard tasks most often between 9-12 AM
        Dictionary<int, EnergyLevel> hourlyPattern = new Dictionary<int, EnergyLevel>
        {
            { 9, EnergyLevel.High }, { 10, EnergyLevel.High }, { 11, EnergyLevel.High },
            { 14, EnergyLevel.Low }, { 15, EnergyLevel.Low },
        };

        // When — 10 AM
        EnergyLevel level = EnergyPatternDetector.InferByHour(hourlyPattern, 10);

        // Then
        level.ShouldBe(EnergyLevel.High);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToMedium_When_HourHasNoPattern()
    {
        Dictionary<int, EnergyLevel> hourlyPattern = new Dictionary<int, EnergyLevel>
        {
            { 9, EnergyLevel.High },
        };
        EnergyPatternDetector.InferByHour(hourlyPattern, 15).ShouldBe(EnergyLevel.Medium);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_PatternIsNull()
    {
        Should.Throw<ArgumentNullException>(() => EnergyPatternDetector.InferByHour(null!, 10));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    [InlineData(100)]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_HourOutOfRange(int hour)
    {
        Dictionary<int, EnergyLevel> pattern = new Dictionary<int, EnergyLevel>();
        DomainException ex = Should.Throw<DomainException>(() => EnergyPatternDetector.InferByHour(pattern, hour));
        ex.Message.ShouldContain("Hour of day");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Accept_HourZero_WithoutThrowing()
    {
        // Kills `hour < 0` -> `hour <= 0` mutation: hour 0 must succeed.
        Dictionary<int, EnergyLevel> pattern = new Dictionary<int, EnergyLevel>();
        EnergyLevel level = EnergyPatternDetector.InferByHour(pattern, 0);
        level.ShouldBe(EnergyLevel.Medium);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Accept_Hour23_WithoutThrowing()
    {
        // Kills `hour > 23` -> `hour >= 23` mutation: hour 23 must succeed.
        Dictionary<int, EnergyLevel> pattern = new Dictionary<int, EnergyLevel>
        {
            { 23, EnergyLevel.Low },
        };
        EnergyLevel level = EnergyPatternDetector.InferByHour(pattern, 23);
        level.ShouldBe(EnergyLevel.Low);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BuildHourlyPattern_When_ValidObservationsProvided()
    {
        Dictionary<int, EnergyLevel> observed = new Dictionary<int, EnergyLevel>
        {
            { 0, EnergyLevel.Low },
            { 10, EnergyLevel.High },
            { 23, EnergyLevel.Medium },
        };

        IReadOnlyDictionary<int, EnergyLevel> pattern = EnergyPatternDetector.BuildHourlyPattern(observed);

        pattern.Count.ShouldBe(3);
        pattern[0].ShouldBe(EnergyLevel.Low);
        pattern[10].ShouldBe(EnergyLevel.High);
        pattern[23].ShouldBe(EnergyLevel.Medium);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_BuildPatternWithInvalidHour()
    {
        Dictionary<int, EnergyLevel> observed = new Dictionary<int, EnergyLevel>
        {
            { 25, EnergyLevel.High },
        };
        DomainException ex = Should.Throw<DomainException>(() => EnergyPatternDetector.BuildHourlyPattern(observed));
        ex.Message.ShouldContain("Hour of day");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_BuildPatternWithNegativeHour()
    {
        Dictionary<int, EnergyLevel> observed = new Dictionary<int, EnergyLevel>
        {
            { -1, EnergyLevel.High },
        };
        Should.Throw<DomainException>(() => EnergyPatternDetector.BuildHourlyPattern(observed));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_BuildPatternNull()
    {
        Should.Throw<ArgumentNullException>(() => EnergyPatternDetector.BuildHourlyPattern(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BuildWeeklyEnergyProfile_When_ConsistentWeeklyCheckIns()
    {
        // Given — consistent High on Mondays, Low on Fridays (pattern across weeks)
        Dictionary<DayOfWeek, EnergyLevel> weeklyCheckIns = new Dictionary<DayOfWeek, EnergyLevel>
        {
            { DayOfWeek.Monday, EnergyLevel.High },
            { DayOfWeek.Tuesday, EnergyLevel.High },
            { DayOfWeek.Wednesday, EnergyLevel.Medium },
            { DayOfWeek.Thursday, EnergyLevel.Medium },
            { DayOfWeek.Friday, EnergyLevel.Low },
            { DayOfWeek.Saturday, EnergyLevel.Medium },
            { DayOfWeek.Sunday, EnergyLevel.Medium },
        };

        // When
        EnergyProfile profile = EnergyProfile.FromCheckIns(weeklyCheckIns);

        // Then
        profile.HasSufficientData.ShouldBeTrue();
        profile.GetTypicalEnergy(DayOfWeek.Monday).ShouldBe(EnergyLevel.High);
        profile.GetTypicalEnergy(DayOfWeek.Friday).ShouldBe(EnergyLevel.Low);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptBoundaryHours_When_BuildingPattern()
    {
        Dictionary<int, EnergyLevel> observed = new Dictionary<int, EnergyLevel>
        {
            { 0, EnergyLevel.Medium },
            { 23, EnergyLevel.Medium },
        };
        IReadOnlyDictionary<int, EnergyLevel> pattern = EnergyPatternDetector.BuildHourlyPattern(observed);
        pattern.Count.ShouldBe(2);
    }
}
