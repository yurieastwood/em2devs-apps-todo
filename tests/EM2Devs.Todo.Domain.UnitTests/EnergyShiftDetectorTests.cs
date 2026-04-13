using Shouldly;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for EnergyShiftDetector, covering mid-day energy shift recommendations.
/// Based on energy-scheduling.feature: mid-day energy shift recommendation.
/// </summary>
public sealed class EnergyShiftDetectorTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuggestLighterTasks_When_MorningWasHighAndDipHourReached()
    {
        // Given — high this morning, dip expected at 2 PM, currently 2 PM
        EnergyShiftRecommendation result = EnergyShiftDetector.Evaluate(
            morningEnergy: EnergyLevel.High,
            currentHour: 14,
            typicalDipHour: 14);

        // Then
        result.ShouldSuggestLighterTasks.ShouldBeTrue();
        result.Message.ShouldNotBeNull();
        result.Message!.ShouldContain("lighter tasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuggestLighterTasks_When_HighMorningAndPastDipHour()
    {
        EnergyShiftRecommendation result = EnergyShiftDetector.Evaluate(EnergyLevel.Peak, 15, 14);
        result.ShouldSuggestLighterTasks.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuggestLighterTasks_When_BeforeDipHour()
    {
        EnergyShiftRecommendation result = EnergyShiftDetector.Evaluate(EnergyLevel.High, 10, 14);
        result.ShouldSuggestLighterTasks.ShouldBeFalse();
        result.Message.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuggestLighterTasks_When_MorningEnergyNotHigh()
    {
        EnergyShiftRecommendation result = EnergyShiftDetector.Evaluate(EnergyLevel.Medium, 14, 14);
        result.ShouldSuggestLighterTasks.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotSuggestLighterTasks_When_MorningEnergyLow()
    {
        EnergyShiftRecommendation result = EnergyShiftDetector.Evaluate(EnergyLevel.Low, 14, 14);
        result.ShouldSuggestLighterTasks.ShouldBeFalse();
    }

    [Theory]
    [InlineData(-1, 14)]
    [InlineData(24, 14)]
    [InlineData(14, -1)]
    [InlineData(14, 24)]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_HourOutOfRange(int currentHour, int dipHour)
    {
        DomainException ex = Should.Throw<DomainException>(() => EnergyShiftDetector.Evaluate(EnergyLevel.High, currentHour, dipHour));
        ex.Message.ShouldContain("must be between 0 and 23");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseSpecificMessage_When_CurrentHourInvalid()
    {
        DomainException ex = Should.Throw<DomainException>(() => EnergyShiftDetector.Evaluate(EnergyLevel.High, 24, 10));
        ex.Message.ShouldContain("Current hour");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseSpecificMessage_When_DipHourInvalid()
    {
        DomainException ex = Should.Throw<DomainException>(() => EnergyShiftDetector.Evaluate(EnergyLevel.High, 10, 24));
        ex.Message.ShouldContain("Typical dip hour");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseLighterTasksMessage_When_SuggestingShift()
    {
        EnergyShiftRecommendation result = EnergyShiftDetector.Evaluate(EnergyLevel.High, 14, 14);
        result.Message.ShouldNotBeNull();
        result.Message!.ShouldContain("Energy usually dips");
        result.Message.ShouldContain("lighter tasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptBoundaryHours_When_Evaluating()
    {
        EnergyShiftRecommendation r1 = EnergyShiftDetector.Evaluate(EnergyLevel.Medium, 0, 0);
        r1.ShouldSuggestLighterTasks.ShouldBeFalse();
        EnergyShiftRecommendation r2 = EnergyShiftDetector.Evaluate(EnergyLevel.High, 23, 23);
        r2.ShouldSuggestLighterTasks.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_NoShiftCreatedWithEmptyMessage()
    {
        Should.Throw<ArgumentException>(() => EnergyShiftRecommendation.SuggestLighterTasks(""));
        Should.Throw<ArgumentException>(() => EnergyShiftRecommendation.SuggestLighterTasks("   "));
        Should.Throw<ArgumentNullException>(() => EnergyShiftRecommendation.SuggestLighterTasks(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNullMessage_When_NoShift()
    {
        EnergyShiftRecommendation none = EnergyShiftRecommendation.NoShift();
        none.ShouldSuggestLighterTasks.ShouldBeFalse();
        none.Message.ShouldBeNull();
    }
}
