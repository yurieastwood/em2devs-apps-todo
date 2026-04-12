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

    // =================================================================
    // Scenario: Manually set energy level (including Peak)
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordPeakEnergy_When_UserChecksInWithPeak()
    {
        // Given / When
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.Peak, DateTimeOffset.UtcNow);

        // Then
        checkIn.Level.ShouldBe(EnergyLevel.Peak);
    }

    // =================================================================
    // Scenario: Skip energy check-in on first day (default to Medium)
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToMedium_When_NewUserSkipsCheckIn()
    {
        // Given — new user with no history dismisses the prompt
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // When
        EnergyCheckIn checkIn = EnergyCheckIn.CreateDefault(now);

        // Then
        checkIn.Level.ShouldBe(EnergyLevel.Medium);
        checkIn.RecordedAt.ShouldBe(now);
        checkIn.Id.Value.ShouldNotBe(Guid.Empty);
    }

    // =================================================================
    // Scenario: New user with insufficient energy data
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseDefaultMedium_When_InsufficientCheckInHistory()
    {
        // Given — fewer than 7 check-ins, system uses defaults
        var checkIns = new List<EnergyCheckIn>
        {
            EnergyCheckIn.Create(EnergyLevel.High, DateTimeOffset.UtcNow.AddDays(-2)),
            EnergyCheckIn.Create(EnergyLevel.Low, DateTimeOffset.UtcNow.AddDays(-1)),
        };

        // When — system determines there's insufficient data
        bool hasSufficientData = checkIns.Count >= EnergyCheckIn.MinimumCheckInsForPattern;

        // Then
        hasSufficientData.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveSufficientData_When_7OrMoreCheckInsExist()
    {
        // Given — exactly 7 check-ins
        var checkIns = new List<EnergyCheckIn>();
        for (int i = 0; i < 7; i++)
        {
            checkIns.Add(EnergyCheckIn.Create(EnergyLevel.Medium, DateTimeOffset.UtcNow.AddDays(-i)));
        }

        // When
        bool hasSufficientData = checkIns.Count >= EnergyCheckIn.MinimumCheckInsForPattern;

        // Then
        hasSufficientData.ShouldBeTrue();
    }

    // =================================================================
    // Scenario: Mid-day re-check-in updates the current level
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UpdateEnergyLevel_When_UserReChecksInMidDay()
    {
        // Given — user checked in as High at 9 AM
        DateTimeOffset morning = new(2026, 4, 12, 9, 0, 0, TimeSpan.Zero);
        EnergyCheckIn morningCheckIn = EnergyCheckIn.Create(EnergyLevel.High, morning);

        // When — user re-checks in at 11 AM as Low
        DateTimeOffset midDay = new(2026, 4, 12, 11, 0, 0, TimeSpan.Zero);
        morningCheckIn.UpdateLevel(EnergyLevel.Low, midDay);

        // Then
        morningCheckIn.Level.ShouldBe(EnergyLevel.Low);
        morningCheckIn.RecordedAt.ShouldBe(midDay);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackPreviousLevel_When_UserReChecksIn()
    {
        // Given
        DateTimeOffset morning = new(2026, 4, 12, 9, 0, 0, TimeSpan.Zero);
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, morning);

        // When
        DateTimeOffset midDay = new(2026, 4, 12, 11, 0, 0, TimeSpan.Zero);
        checkIn.UpdateLevel(EnergyLevel.Low, midDay);

        // Then — should track the previous level for pattern analysis
        checkIn.PreviousLevel.ShouldBe(EnergyLevel.High);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullPreviousLevel_When_NeverUpdated()
    {
        // Given / When
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, DateTimeOffset.UtcNow);

        // Then
        checkIn.PreviousLevel.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IndicateRapidFluctuation_When_EnergyChangedMidDay()
    {
        // Given — user checked in as High at 9 AM
        DateTimeOffset morning = new(2026, 4, 12, 9, 0, 0, TimeSpan.Zero);
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, morning);

        // When — user re-checks in at 11 AM as Low
        DateTimeOffset midDay = new(2026, 4, 12, 11, 0, 0, TimeSpan.Zero);
        checkIn.UpdateLevel(EnergyLevel.Low, midDay);

        // Then — should flag this as a rapid fluctuation
        checkIn.HasFluctuated.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotIndicateFluctuation_When_NeverUpdated()
    {
        // Given / When
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, DateTimeOffset.UtcNow);

        // Then
        checkIn.HasFluctuated.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepLatestLevel_When_MultipleUpdates()
    {
        // Given
        DateTimeOffset morning = new(2026, 4, 12, 9, 0, 0, TimeSpan.Zero);
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, morning);

        // When — multiple updates throughout the day
        DateTimeOffset midDay = new(2026, 4, 12, 11, 0, 0, TimeSpan.Zero);
        checkIn.UpdateLevel(EnergyLevel.Medium, midDay);
        DateTimeOffset afternoon = new(2026, 4, 12, 14, 0, 0, TimeSpan.Zero);
        checkIn.UpdateLevel(EnergyLevel.Low, afternoon);

        // Then — only the latest counts
        checkIn.Level.ShouldBe(EnergyLevel.Low);
        checkIn.RecordedAt.ShouldBe(afternoon);
        checkIn.PreviousLevel.ShouldBe(EnergyLevel.Medium);
    }
}
