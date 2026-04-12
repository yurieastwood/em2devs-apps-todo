using Shouldly;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for OvercommitmentWarning value object.
/// Tests encode warning behaviors from capacity-modelling.feature.
/// </summary>
public sealed class OvercommitmentWarningTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWarning_When_OvercommitmentDetected()
    {
        // When
        var warning = OvercommitmentWarning.Create(
            DayOfWeek.Wednesday,
            typicalCapacityUnits: 18,
            scheduledTaskCount: 10,
            scheduledUnits: 30);

        // Then
        warning.Day.ShouldBe(DayOfWeek.Wednesday);
        warning.TypicalCapacityUnits.ShouldBe(18);
        warning.ScheduledTaskCount.ShouldBe(10);
        warning.ScheduledUnits.ShouldBe(30);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainDayName_When_MessageGenerated()
    {
        // When
        var warning = OvercommitmentWarning.Create(
            DayOfWeek.Friday,
            typicalCapacityUnits: 18,
            scheduledTaskCount: 8,
            scheduledUnits: 24);

        // Then
        warning.Message.ShouldContain("Friday");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainCapacityAndScheduledCounts_When_MessageGenerated()
    {
        // When
        var warning = OvercommitmentWarning.Create(
            DayOfWeek.Monday,
            typicalCapacityUnits: 18,
            scheduledTaskCount: 10,
            scheduledUnits: 30);

        // Then
        warning.Message.ShouldContain("18");
        warning.Message.ShouldContain("10");
        warning.Message.ShouldContain("30");
        warning.Message.ShouldContain("Consider reprioritising");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeValueEqual_When_SameProperties()
    {
        // Given
        var warning1 = OvercommitmentWarning.Create(DayOfWeek.Monday, 18, 10, 30);
        var warning2 = OvercommitmentWarning.Create(DayOfWeek.Monday, 18, 10, 30);

        // Then (record equality)
        warning1.ShouldBe(warning2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeValueEqual_When_DifferentProperties()
    {
        // Given
        var warning1 = OvercommitmentWarning.Create(DayOfWeek.Monday, 18, 10, 30);
        var warning2 = OvercommitmentWarning.Create(DayOfWeek.Tuesday, 18, 10, 30);

        // Then
        warning1.ShouldNotBe(warning2);
    }
}
