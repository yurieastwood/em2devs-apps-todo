using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for CapacityDismissalTracker, covering warning reduction after repeated dismissals.
/// Based on capacity-modelling.feature: capacity warnings dismissed repeatedly.
/// </summary>
public sealed class CapacityDismissalTrackerTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroDismissals_When_StartingNewWeek()
    {
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(new DateOnly(2026, 4, 13));
        tracker.DismissalCount.ShouldBe(0);
        tracker.ShouldReduceWarning.ShouldBeFalse();
        tracker.WeekStart.ShouldBe(new DateOnly(2026, 4, 13));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementDismissals_When_RecordingWithinWeek()
    {
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday);

        tracker = tracker.RecordDismissal(monday);
        tracker.DismissalCount.ShouldBe(1);
        tracker.ShouldReduceWarning.ShouldBeFalse();

        tracker = tracker.RecordDismissal(monday.AddDays(1));
        tracker.DismissalCount.ShouldBe(2);
        tracker.ShouldReduceWarning.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReduceWarning_When_ThresholdReached()
    {
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday)
            .RecordDismissal(monday)
            .RecordDismissal(monday)
            .RecordDismissal(monday);

        tracker.DismissalCount.ShouldBe(3);
        tracker.ShouldReduceWarning.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetWeek_When_DismissalAfterWeekEnd()
    {
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday)
            .RecordDismissal(monday)
            .RecordDismissal(monday)
            .RecordDismissal(monday);

        // Next Monday
        DateOnly nextMonday = monday.AddDays(7);
        tracker = tracker.RecordDismissal(nextMonday);
        tracker.DismissalCount.ShouldBe(1);
        tracker.ShouldReduceWarning.ShouldBeFalse();
        tracker.WeekStart.ShouldBe(nextMonday);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_DismissalDateBeforeWeekStart()
    {
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday);
        DomainException ex = Should.Throw<DomainException>(() => tracker.RecordDismissal(monday.AddDays(-1)));
        ex.Message.ShouldContain("precede");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResetOnNonMondayDate_ToStartOfItsWeek()
    {
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday);

        // Two weeks later, Thursday
        DateOnly laterThursday = new DateOnly(2026, 4, 30);
        CapacityDismissalTracker next = tracker.RecordDismissal(laterThursday);
        next.DismissalCount.ShouldBe(1);
        // Start of week for April 30 2026 (Thursday) is Monday April 27
        next.WeekStart.ShouldBe(new DateOnly(2026, 4, 27));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepRecordingWithinSameWeek_AtEndBoundary()
    {
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday);

        // Sunday is still in the same week window (Mon -> Mon exclusive)
        DateOnly sunday = monday.AddDays(6);
        CapacityDismissalTracker updated = tracker.RecordDismissal(sunday);
        updated.DismissalCount.ShouldBe(1);
        updated.WeekStart.ShouldBe(monday);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeThresholdConstant()
    {
        CapacityDismissalTracker.DismissalThreshold.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotReduceWarning_JustBelowThreshold()
    {
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday)
            .RecordDismissal(monday)
            .RecordDismissal(monday);
        tracker.ShouldReduceWarning.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ComputeStartOfWeek_ForSundayDate()
    {
        // Start-of-week algorithm must handle Sunday (which is the last day of the prior Monday-week).
        DateOnly monday = new DateOnly(2026, 4, 13);
        CapacityDismissalTracker tracker = CapacityDismissalTracker.StartWeek(monday);

        // A Sunday two weeks later: 2026-04-26 (Sun) -> week start Monday 2026-04-20
        DateOnly sunday = new DateOnly(2026, 4, 26);
        CapacityDismissalTracker next = tracker.RecordDismissal(sunday);
        next.WeekStart.ShouldBe(new DateOnly(2026, 4, 20));
    }
}
