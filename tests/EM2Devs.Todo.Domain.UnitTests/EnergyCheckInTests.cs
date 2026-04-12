using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for EnergyCheckIn entity.
/// Tests encode behaviors from energy-scheduling.feature (ADR-0003).
/// </summary>
public sealed class EnergyCheckInTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordHighEnergy_When_UserChecksInWithHigh()
    {
        // Given
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // When
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, now);

        // Then
        checkIn.Id.Value.ShouldNotBe(Guid.Empty);
        checkIn.Level.ShouldBe(EnergyLevel.High);
        checkIn.RecordedAt.ShouldBe(now);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordMediumEnergy_When_UserChecksInWithMedium()
    {
        // Given / When
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.Medium, DateTimeOffset.UtcNow);

        // Then
        checkIn.Level.ShouldBe(EnergyLevel.Medium);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordLowEnergy_When_UserChecksInWithLow()
    {
        // Given / When
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.Low, DateTimeOffset.UtcNow);

        // Then
        checkIn.Level.ShouldBe(EnergyLevel.Low);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToMedium_When_InferredWithNoHistory()
    {
        // Given / When
        EnergyCheckIn checkIn = EnergyCheckIn.CreateDefault(DateTimeOffset.UtcNow);

        // Then
        checkIn.Level.ShouldBe(EnergyLevel.Medium);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveTimestamp_When_CheckInCreated()
    {
        // Given
        DateTimeOffset specificTime = new(2026, 3, 15, 9, 0, 0, TimeSpan.Zero);

        // When
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, specificTime);

        // Then
        checkIn.RecordedAt.ShouldBe(specificTime);
    }
}
