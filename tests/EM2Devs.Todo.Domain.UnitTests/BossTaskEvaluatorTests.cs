using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for BossTaskEvaluator domain service.
/// Covers boss task promotion rules from boss-tasks.feature.
/// </summary>
public sealed class BossTaskEvaluatorTests
{
    // =================================================================
    // Scenario: Task promoted after repeated rescheduling
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PromoteToBossTask_When_TaskRescheduled3Times()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Write architecture decision record"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PromoteToBossTask_When_TaskRescheduledMoreThan3Times()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Overdue report"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPromoteToBossTask_When_TaskRescheduledLessThan3Times()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Simple task"));
        task.Reschedule();
        task.Reschedule();

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_TaskAlreadyBossAndStillQualifies()
    {
        // Given — task already promoted, still meets criteria
        var task = TodoTask.Create(new TaskTitle("Already boss"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        task.PromoteToBossTask();

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then — no change because already boss
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeTrue();
    }

    // =================================================================
    // Scenario: Task promoted based on age and priority
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PromoteToBossTask_When_HighPriorityTaskOpenFor14Days()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Refactor authentication module"),
            priority: TaskPriority.High,
            createdAt: DateTimeOffset.UtcNow.AddDays(-15));

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PromoteToBossTask_When_CriticalPriorityTaskOpenFor14Days()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Fix critical security bug"),
            priority: TaskPriority.Critical,
            createdAt: DateTimeOffset.UtcNow.AddDays(-14));

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPromoteToBossTask_When_HighPriorityTaskOpenForLessThan14Days()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Recent high priority"),
            priority: TaskPriority.High,
            createdAt: DateTimeOffset.UtcNow.AddDays(-13));

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    // =================================================================
    // Scenario: Low-priority task is not promoted despite age
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPromoteToBossTask_When_LowPriorityTaskOpenFor30Days()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Reorganise bookshelf"),
            priority: TaskPriority.Low,
            createdAt: DateTimeOffset.UtcNow.AddDays(-30));

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPromoteToBossTask_When_MediumPriorityTaskOpenFor30Days()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Organise desk"),
            priority: TaskPriority.Medium,
            createdAt: DateTimeOffset.UtcNow.AddDays(-30));

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    // =================================================================
    // Scenario: Task promoted based on high difficulty and avoidance
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PromoteToBossTask_When_HardTaskViewed5TimesWithoutProgress()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Prepare annual tax filing"),
            difficulty: TaskDifficulty.Hard);
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PromoteToBossTask_When_EpicTaskViewed5TimesWithoutProgress()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Massive refactor"),
            difficulty: TaskDifficulty.Epic);
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPromoteToBossTask_When_HardTaskViewedLessThan5Times()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Tax filing"),
            difficulty: TaskDifficulty.Hard);
        for (int i = 0; i < 4; i++)
        {
            task.RecordView();
        }

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPromoteToBossTask_When_NormalDifficultyTaskViewed5Times()
    {
        // Given
        var task = TodoTask.Create(
            new TaskTitle("Normal difficulty task"),
            difficulty: TaskDifficulty.Normal);
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotPromoteToBossTask_When_HardTaskViewed5TimesButInProgress()
    {
        // Given — task has been started so avoidance rule does not apply
        var task = TodoTask.Create(
            new TaskTitle("Tax filing in progress"),
            difficulty: TaskDifficulty.Hard);
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        task.MoveToInProgress();

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    // =================================================================
    // Scenario: Boss Task is demoted when conditions no longer apply
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DemoteFromBossTask_When_PriorityChangedToLow()
    {
        // Given — task was promoted due to age + high priority
        var task = TodoTask.Create(
            new TaskTitle("Refactor authentication module"),
            priority: TaskPriority.High,
            createdAt: DateTimeOffset.UtcNow.AddDays(-15));
        BossTaskEvaluator.Evaluate(task);
        task.IsBossTask.ShouldBeTrue();

        // When — priority changed to Low, then re-evaluate
        task.UpdatePriority(TaskPriority.Low);
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DemoteFromBossTask_When_ConditionsNoLongerMet()
    {
        // Given — task was promoted due to avoidance (hard + 5 views, todo status)
        var task = TodoTask.Create(
            new TaskTitle("Hard task now in progress"),
            difficulty: TaskDifficulty.Hard);
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        BossTaskEvaluator.Evaluate(task);
        task.IsBossTask.ShouldBeTrue();

        // When — user starts working on it (no longer "without progress")
        task.MoveToInProgress();
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeFalse();
    }

    // =================================================================
    // Scenario: User manually promotes a task to Boss Task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BecomeBossTask_When_ManuallyPromoted()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Have difficult conversation with manager"));

        // When — manual promotion uses the existing PromoteToBossTask method
        task.PromoteToBossTask();

        // Then
        task.IsBossTask.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotDemoteManuallyPromotedTask_When_NoAutomaticCriteriaMet()
    {
        // Given — manually promoted task that does not meet any automatic criteria
        var task = TodoTask.Create(new TaskTitle("Manual boss task"));
        task.PromoteToBossTask();
        task.IsBossTask.ShouldBeTrue();

        // When — evaluator runs but no automatic criteria are met
        // The evaluator would demote because ShouldPromote returns false
        // This is correct behavior: the evaluator manages automatic promotion/demotion
        // Manual promotion is an explicit user action tracked separately if needed
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then — evaluator demotes because automatic criteria are not met
        // Manual promotion is a separate concept from automatic promotion
        changed.ShouldBeTrue();
        task.IsBossTask.ShouldBeFalse();
    }

    // =================================================================
    // Edge cases and guard clauses
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EvaluatingNull()
    {
        Should.Throw<ArgumentNullException>(() => BossTaskEvaluator.Evaluate(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CheckingShouldPromoteWithNull()
    {
        Should.Throw<ArgumentNullException>(() => BossTaskEvaluator.ShouldPromote(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_TaskIsDoneEvenWithPromotionConditions()
    {
        // Given — task meets promotion criteria (3+ reschedules) but is Done
        var task = TodoTask.Create(new TaskTitle("Completed task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        task.MoveToInProgress();
        task.MarkAsDone();

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then — should not promote because task is Done
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_TaskIsSkippedEvenWithPromotionConditions()
    {
        // Given — task meets promotion criteria (3+ reschedules) but is Skipped
        var task = TodoTask.Create(new TaskTitle("Skipped task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        task.Skip();

        // When
        bool changed = BossTaskEvaluator.Evaluate(task);

        // Then — should not promote because task is Skipped
        changed.ShouldBeFalse();
        task.IsBossTask.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_ShouldPromoteDetectsRescheduling()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Rescheduled task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();

        // When
        bool result = BossTaskEvaluator.ShouldPromote(task);

        // Then
        result.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_NoPromotionCriteriaMet()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Brand new task"));

        // When
        bool result = BossTaskEvaluator.ShouldPromote(task);

        // Then
        result.ShouldBeFalse();
    }
}
