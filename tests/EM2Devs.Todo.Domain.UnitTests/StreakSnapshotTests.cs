using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Tests for the StreakSnapshot entity factory.
/// Used by DailyStreakEvaluationJob to capture end-of-day streak state.
/// </summary>
public sealed class StreakSnapshotTests
{
    private static readonly DateOnly _snapshotDate = new(2026, 4, 7);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PopulateAllFields_When_CaptureCalled()
    {
        // Given
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // When
        var snapshot = StreakSnapshot.Capture(
            snapshotDate: _snapshotDate,
            currentDays: 5,
            longestDays: 12,
            graceDaysAvailable: 2,
            wasActive: true);

        // Then
        snapshot.Id.ShouldNotBe(Guid.Empty);
        snapshot.SnapshotDate.ShouldBe(_snapshotDate);
        snapshot.CurrentDays.ShouldBe(5);
        snapshot.LongestDays.ShouldBe(12);
        snapshot.GraceDaysAvailable.ShouldBe(2);
        snapshot.WasActive.ShouldBeTrue();
        snapshot.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateDistinctIds_When_TwoSnapshotsCaptured()
    {
        // Given / When
        var first = StreakSnapshot.Capture(_snapshotDate, 0, 0, 0, wasActive: false);
        var second = StreakSnapshot.Capture(_snapshotDate, 0, 0, 0, wasActive: false);

        // Then — Guid.NewGuid() produces unique values
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CarryWasActiveFalse_When_SnapshotCapturedForInactiveDay()
    {
        // Given / When
        var snapshot = StreakSnapshot.Capture(
            snapshotDate: _snapshotDate,
            currentDays: 0,
            longestDays: 0,
            graceDaysAvailable: 0,
            wasActive: false);

        // Then
        snapshot.WasActive.ShouldBeFalse();
        snapshot.CurrentDays.ShouldBe(0);
    }
}
