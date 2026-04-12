using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for DailyBrief entity.
/// Tests encode behaviors from daily-brief.feature (ADR-0003).
/// </summary>
public sealed class DailyBriefTests
{
    private static readonly DateOnly _today = new(2026, 3, 15);

    // ── Scenario 1: Daily brief generated on first login of the day ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithCorePlanAndIfTimeAllows_When_BriefGenerated()
    {
        // Given
        List<TaskId> corePlan = [TaskId.New(), TaskId.New(), TaskId.New()];
        List<TaskId> ifTimeAllows = [TaskId.New(), TaskId.New()];
        int capacity = 3;

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, ifTimeAllows, capacity);

        // Then
        brief.Id.Value.ShouldNotBe(Guid.Empty);
        brief.Date.ShouldBe(_today);
        brief.CorePlan.Count.ShouldBe(3);
        brief.IfTimeAllows.Count.ShouldBe(2);
        brief.Status.ShouldBe(DailyBriefStatus.Generated);
        brief.Capacity.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithGeneratedStatus_When_BriefCreated()
    {
        // Given
        List<TaskId> corePlan = [TaskId.New(), TaskId.New(), TaskId.New()];

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, [], 6);

        // Then
        brief.Id.Value.ShouldNotBe(Guid.Empty);
        brief.Date.ShouldBe(_today);
        brief.CorePlan.Count.ShouldBe(3);
        brief.Status.ShouldBe(DailyBriefStatus.Generated);
    }

    // ── Scenario 2: Daily brief without calendar integration ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateWithoutCalendarIntegration_When_NoCalendarEnabled()
    {
        // Given
        List<TaskId> corePlan = [TaskId.New(), TaskId.New(), TaskId.New()];

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, [], 6);

        // Then
        brief.HasCalendarIntegration.ShouldBeFalse();
        brief.CalendarBlockMinutes.ShouldBe(0);
    }

    // ── Scenario 3: Daily brief highlights overdue tasks ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HighlightOverdueTasks_When_OverdueTasksExist()
    {
        // Given
        List<TaskId> overdue = [TaskId.New(), TaskId.New(), TaskId.New()];
        List<TaskId> corePlan = [TaskId.New(), TaskId.New()];

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, [], 6, overdue);

        // Then
        brief.OverdueTasks.Count.ShouldBe(3);
        brief.HasOverdueTasks.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoOverdueTasks_When_NoneProvided()
    {
        // Given
        List<TaskId> corePlan = [TaskId.New(), TaskId.New()];

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, [], 6);

        // Then
        brief.OverdueTasks.Count.ShouldBe(0);
        brief.HasOverdueTasks.ShouldBeFalse();
    }

    // ── Scenario 4: Daily brief respects capacity model ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecommendCapacityTasksAsCorePlan_When_TasksExceedCapacity()
    {
        // Given: capacity is 6, we have 6 core + 4 overflow
        List<TaskId> corePlan = Enumerable.Range(0, 6).Select(_ => TaskId.New()).ToList();
        List<TaskId> ifTimeAllows = Enumerable.Range(0, 4).Select(_ => TaskId.New()).ToList();
        int capacity = 6;

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, ifTimeAllows, capacity);

        // Then
        brief.CorePlan.Count.ShouldBe(6);
        brief.IfTimeAllows.Count.ShouldBe(4);
        brief.ExceedsCapacity.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotExceedCapacity_When_CorePlanMatchesCapacity()
    {
        // Given
        List<TaskId> corePlan = Enumerable.Range(0, 6).Select(_ => TaskId.New()).ToList();

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, [], 6);

        // Then
        brief.ExceedsCapacity.ShouldBeFalse();
    }

    // ── Scenario 5: Brief not generated when insufficient tasks ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FewerThanTwoTasksExist()
    {
        // Given: only 1 task
        List<TaskId> corePlan = [TaskId.New()];

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(_today, corePlan, [], 6));
        ex.Message.ShouldContain("at least 2");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CreatedWithEmptyTaskList()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(_today, [], [], 6));
        ex.Message.ShouldContain("at least 2");
    }

    // ── Scenario 6: Accept the daily brief ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToAccepted_When_Accepted()
    {
        // Given
        DailyBrief brief = CreateValidBrief();

        // When
        brief.Accept();

        // Then
        brief.Status.ShouldBe(DailyBriefStatus.Accepted);
    }

    // ── Scenario 7: Modify the daily brief ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToModified_When_Modified()
    {
        // Given
        DailyBrief brief = CreateValidBrief();
        List<TaskId> newCorePlan = [TaskId.New(), TaskId.New(), TaskId.New()];
        List<TaskId> newIfTimeAllows = [TaskId.New()];

        // When
        brief.Modify(newCorePlan, newIfTimeAllows);

        // Then
        brief.Status.ShouldBe(DailyBriefStatus.Modified);
        brief.CorePlan.Count.ShouldBe(3);
        brief.IfTimeAllows.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ModifiedWithEmptyCorePlan()
    {
        // Given
        DailyBrief brief = CreateValidBrief();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => brief.Modify([], []));
        ex.Message.ShouldContain("at least one");
    }

    // ── Scenario 8: User modifies brief to exceed capacity ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowCapacityWarning_When_ModifiedToExceedCapacity()
    {
        // Given: capacity is 6
        DailyBrief brief = CreateBriefWithCapacity(6);
        List<TaskId> expandedCorePlan = Enumerable.Range(0, 8).Select(_ => TaskId.New()).ToList();

        // When
        brief.Modify(expandedCorePlan, []);

        // Then
        brief.ExceedsCapacity.ShouldBeTrue();
        brief.CapacityWarning.ShouldNotBeNull();
        brief.CapacityWarning.ShouldContain("exceeds your typical daily capacity of 6 tasks");
        brief.Status.ShouldBe(DailyBriefStatus.Modified);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotShowCapacityWarning_When_ModifiedWithinCapacity()
    {
        // Given
        DailyBrief brief = CreateBriefWithCapacity(6);
        List<TaskId> corePlan = Enumerable.Range(0, 5).Select(_ => TaskId.New()).ToList();

        // When
        brief.Modify(corePlan, []);

        // Then
        brief.ExceedsCapacity.ShouldBeFalse();
        brief.CapacityWarning.ShouldBeNull();
    }

    // ── Scenario 9: Dismiss the daily brief ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TransitionToDismissed_When_Dismissed()
    {
        // Given
        DailyBrief brief = CreateValidBrief();

        // When
        brief.Dismiss();

        // Then
        brief.Status.ShouldBe(DailyBriefStatus.Dismissed);
    }

    // ── Scenario 10: Brief accuracy improves with feedback ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementAcceptCount_When_Accepted()
    {
        // Given
        DailyBrief brief = CreateValidBrief(acceptCount: 5, modifyCount: 2, dismissCount: 1);

        // When
        brief.Accept();

        // Then
        brief.FeedbackAcceptCount.ShouldBe(6);
        brief.FeedbackModifyCount.ShouldBe(2);
        brief.FeedbackDismissCount.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementModifyCount_When_Modified()
    {
        // Given
        DailyBrief brief = CreateValidBrief(acceptCount: 3, modifyCount: 4, dismissCount: 0);
        List<TaskId> newPlan = [TaskId.New(), TaskId.New()];

        // When
        brief.Modify(newPlan, []);

        // Then
        brief.FeedbackAcceptCount.ShouldBe(3);
        brief.FeedbackModifyCount.ShouldBe(5);
        brief.FeedbackDismissCount.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementDismissCount_When_Dismissed()
    {
        // Given
        DailyBrief brief = CreateValidBrief(acceptCount: 1, modifyCount: 2, dismissCount: 3);

        // When
        brief.Dismiss();

        // Then
        brief.FeedbackAcceptCount.ShouldBe(1);
        brief.FeedbackModifyCount.ShouldBe(2);
        brief.FeedbackDismissCount.ShouldBe(4);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TrackZeroFeedbackCounts_When_NewBrief()
    {
        // Given / When
        DailyBrief brief = CreateValidBrief();

        // Then
        brief.FeedbackAcceptCount.ShouldBe(0);
        brief.FeedbackModifyCount.ShouldBe(0);
        brief.FeedbackDismissCount.ShouldBe(0);
    }

    // ── Scenario 11: Daily brief factors in calendar blocks (Premium) ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReduceAvailableCapacity_When_CalendarBlocksProvided()
    {
        // Given: 2 hours of meetings (120 minutes), capacity 6
        List<TaskId> corePlan = [TaskId.New(), TaskId.New(), TaskId.New()];

        // When
        DailyBrief brief = DailyBrief.Create(
            _today, corePlan, [], 6, overdueTasks: [],
            hasCalendarIntegration: true, calendarBlockMinutes: 120);

        // Then
        brief.HasCalendarIntegration.ShouldBeTrue();
        brief.CalendarBlockMinutes.ShouldBe(120);
    }

    // ── Validation edge cases ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatedWithNullCorePlan()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(() =>
            DailyBrief.Create(_today, null!, [], 6));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatedWithNullIfTimeAllows()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(() =>
            DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], null!, 6));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ModifiedWithNullCorePlan()
    {
        // Given
        DailyBrief brief = CreateValidBrief();

        // When / Then
        Should.Throw<ArgumentNullException>(() => brief.Modify(null!, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ModifiedWithNullIfTimeAllows()
    {
        // Given
        DailyBrief brief = CreateValidBrief();

        // When / Then
        Should.Throw<ArgumentNullException>(() => brief.Modify([TaskId.New()], null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CapacityIsNegative()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], [], -1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CalendarBlockMinutesAreNegative()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], [], 6,
                overdueTasks: [], hasCalendarIntegration: true, calendarBlockMinutes: -30));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToEmptyOverdueTasks_When_OverdueTasksNotProvided()
    {
        // Given / When
        DailyBrief brief = DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], [], 6);

        // Then
        brief.OverdueTasks.Count.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowZeroCapacity_When_Creating()
    {
        // Given / When
        DailyBrief brief = DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], [], 0);

        // Then
        brief.Capacity.ShouldBe(0);
        brief.ExceedsCapacity.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CountTotalTasks_When_CorePlanAndIfTimeAllowsCombined()
    {
        // Given
        List<TaskId> corePlan = [TaskId.New(), TaskId.New(), TaskId.New()];
        List<TaskId> ifTimeAllows = [TaskId.New(), TaskId.New()];

        // When
        DailyBrief brief = DailyBrief.Create(_today, corePlan, ifTimeAllows, 6);

        // Then
        brief.TotalTaskCount.ShouldBe(5);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_FeedbackCountsAreNegative()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], [], 6,
                overdueTasks: [], hasCalendarIntegration: false, calendarBlockMinutes: 0,
                acceptCount: -1, modifyCount: 0, dismissCount: 0));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ModifyFeedbackCountIsNegative()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], [], 6,
                overdueTasks: [], hasCalendarIntegration: false, calendarBlockMinutes: 0,
                acceptCount: 0, modifyCount: -1, dismissCount: 0));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DismissFeedbackCountIsNegative()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() =>
            DailyBrief.Create(_today, [TaskId.New(), TaskId.New()], [], 6,
                overdueTasks: [], hasCalendarIntegration: false, calendarBlockMinutes: 0,
                acceptCount: 0, modifyCount: 0, dismissCount: -1));
        ex.Message.ShouldContain("cannot be negative");
    }

    // ── Helper methods ──

    private static DailyBrief CreateValidBrief(
        int acceptCount = 0, int modifyCount = 0, int dismissCount = 0)
    {
        List<TaskId> corePlan = [TaskId.New(), TaskId.New()];
        return DailyBrief.Create(_today, corePlan, [], 6,
            overdueTasks: [], hasCalendarIntegration: false, calendarBlockMinutes: 0,
            acceptCount: acceptCount, modifyCount: modifyCount, dismissCount: dismissCount);
    }

    private static DailyBrief CreateBriefWithCapacity(int capacity)
    {
        List<TaskId> corePlan = Enumerable.Range(0, Math.Max(2, capacity))
            .Select(_ => TaskId.New()).ToList();
        return DailyBrief.Create(_today, corePlan, [], capacity);
    }
}
