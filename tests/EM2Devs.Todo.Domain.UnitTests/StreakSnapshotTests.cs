using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Tests for the StreakSnapshot entity factory.
/// Used by DailyStreakEvaluationJob to capture end-of-day streak state.
/// </summary>
public sealed class StreakSnapshotTests
{
    private static readonly DateOnly _snapshotDate = new(2026, 4, 7);
    private static readonly Guid _userId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PopulateAllFields_When_CaptureCalled()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;

        var snapshot = StreakSnapshot.Capture(
            userId: _userId,
            snapshotDate: _snapshotDate,
            currentDays: 5,
            longestDays: 12,
            graceDaysAvailable: 2,
            wasActive: true);

        snapshot.Id.ShouldNotBe(Guid.Empty);
        snapshot.UserId.ShouldBe(_userId);
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
        var first = StreakSnapshot.Capture(_userId, _snapshotDate, 0, 0, 0, wasActive: false);
        var second = StreakSnapshot.Capture(_userId, _snapshotDate, 0, 0, 0, wasActive: false);

        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CarryWasActiveFalse_When_SnapshotCapturedForInactiveDay()
    {
        var snapshot = StreakSnapshot.Capture(
            userId: _userId,
            snapshotDate: _snapshotDate,
            currentDays: 0,
            longestDays: 0,
            graceDaysAvailable: 0,
            wasActive: false);

        snapshot.WasActive.ShouldBeFalse();
        snapshot.CurrentDays.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_CaptureCalledWithEmptyUserId()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            StreakSnapshot.Capture(Guid.Empty, _snapshotDate, 0, 0, 0, wasActive: false));
        ex.Message.ShouldBe("UserId cannot be empty.");
    }
}
