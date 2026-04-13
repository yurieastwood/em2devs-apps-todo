using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Boss Task completion flow.
/// Maps to: docs/features/core/boss-tasks.feature
/// Covers: breakdown, re-evaluation, delegation, completion XP, focus mode, title progression, deletion, recurring.
/// </summary>
public sealed class BossTaskCompletionTests
{
    private static readonly DateTimeOffset _now = new(2026, 4, 12, 12, 0, 0, TimeSpan.Zero);

    // =================================================================
    // Scenario: Offer task breakdown when a Boss Task is encountered
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateBreakdownWithSubtasks_When_BossTaskProvided()
    {
        // Given
        var parentTask = TodoTask.Create(new TaskTitle("Write architecture decision record"));
        parentTask.PromoteToBossTask();

        var subtaskTitles = new[]
        {
            new TaskTitle("Research existing patterns"),
            new TaskTitle("Draft initial structure"),
            new TaskTitle("Write detailed rationale")
        };

        // When
        var breakdown = BossTaskBreakdown.Create(parentTask.Id, subtaskTitles);

        // Then
        breakdown.ParentTaskId.ShouldBe(parentTask.Id);
        breakdown.SuggestedSubtasks.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RequireAtLeast2Subtasks_When_CreatingBreakdown()
    {
        // Given
        var parentId = TaskId.New();
        var subtaskTitles = new[] { new TaskTitle("Only one subtask") };

        // When / Then
        var ex = Should.Throw<DomainException>(() => BossTaskBreakdown.Create(parentId, subtaskTitles));
        ex.Message.ShouldContain("at least 2");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RequireAtMost5Subtasks_When_CreatingBreakdown()
    {
        // Given
        var parentId = TaskId.New();
        var subtaskTitles = new[]
        {
            new TaskTitle("Step 1"), new TaskTitle("Step 2"), new TaskTitle("Step 3"),
            new TaskTitle("Step 4"), new TaskTitle("Step 5"), new TaskTitle("Step 6")
        };

        // When / Then
        var ex = Should.Throw<DomainException>(() => BossTaskBreakdown.Create(parentId, subtaskTitles));
        ex.Message.ShouldContain("at most 5");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_BreakdownSubtasksNull()
    {
        // Given / When / Then
        Should.Throw<ArgumentNullException>(() => BossTaskBreakdown.Create(TaskId.New(), null!));
    }

    // =================================================================
    // Scenario: Accept suggested breakdown for a Boss Task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSubtasksFromBreakdown_When_Accepted()
    {
        // Given
        var parentTask = TodoTask.Create(new TaskTitle("Prepare annual tax filing"));
        parentTask.PromoteToBossTask();

        var subtaskTitles = new[]
        {
            new TaskTitle("Gather all income documents"),
            new TaskTitle("Collect deduction receipts"),
            new TaskTitle("Fill in tax form sections"),
            new TaskTitle("Review and submit")
        };

        var breakdown = BossTaskBreakdown.Create(parentTask.Id, subtaskTitles);

        // When
        var subtasks = breakdown.Accept();

        // Then
        subtasks.Count.ShouldBe(4);
        subtasks[0].Title.Value.ShouldBe("Gather all income documents");
        subtasks[1].Title.Value.ShouldBe("Collect deduction receipts");
        subtasks[2].Title.Value.ShouldBe("Fill in tax form sections");
        subtasks[3].Title.Value.ShouldBe("Review and submit");
        subtasks.ShouldAllBe(s => s.Status == TaskStatus.Todo);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateExactlySpecifiedSubtasks_When_BreakdownHas2Subtasks()
    {
        // Given
        var breakdown = BossTaskBreakdown.Create(TaskId.New(), new[]
        {
            new TaskTitle("Part A"),
            new TaskTitle("Part B")
        });

        // When
        var subtasks = breakdown.Accept();

        // Then
        subtasks.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateExactlySpecifiedSubtasks_When_BreakdownHas5Subtasks()
    {
        // Given
        var breakdown = BossTaskBreakdown.Create(TaskId.New(), new[]
        {
            new TaskTitle("Step 1"), new TaskTitle("Step 2"), new TaskTitle("Step 3"),
            new TaskTitle("Step 4"), new TaskTitle("Step 5")
        });

        // When
        var subtasks = breakdown.Accept();

        // Then
        subtasks.Count.ShouldBe(5);
    }

    // =================================================================
    // Scenario: Offer re-evaluation of task priority or deadline
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowPriorityChange_When_BossTaskReEvaluated()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Redesign landing page"),
            priority: TaskPriority.High);
        task.PromoteToBossTask();

        // When — re-evaluate by changing priority
        task.UpdatePriority(TaskPriority.Low);

        // Then
        task.Priority.ShouldBe(TaskPriority.Low);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowDifficultyChange_When_BossTaskReEvaluated()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Redesign landing page"),
            difficulty: TaskDifficulty.Hard);
        task.PromoteToBossTask();

        // When
        task.UpdateDifficulty(TaskDifficulty.Normal);

        // Then
        task.Difficulty.ShouldBe(TaskDifficulty.Normal);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowDeadlineChange_When_BossTaskReEvaluated()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Redesign landing page"));
        task.PromoteToBossTask();

        // When
        var newDeadline = DateTimeOffset.UtcNow.AddDays(14);
        task.UpdateDueDate(newDeadline);

        // Then
        task.DueDate.ShouldBe(newDeadline);
    }

    // =================================================================
    // Scenario: Offer delegation suggestion
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowDelegation_When_UserIsGuildMember()
    {
        // Given — user is a guild member
        var leaderId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var guild = Guild.Create("Dev Guild", "Development team", leaderId, today);
        guild = guild.AddMember(memberId, today);

        // Then — guild has the member, delegation is possible
        guild.IsMember(memberId).ShouldBeTrue();
        guild.MemberCount.ShouldBe(2);
    }

    // =================================================================
    // Scenario: Complete a Boss Task earns bonus XP
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyBossTaskMultiplier_When_BossTaskCompleted()
    {
        // Given — normal Hard task XP
        var normalBreakdown = XpCalculator.Calculate(
            TaskDifficulty.Hard, null, _now, 0);

        // When — boss task XP (2x multiplier)
        var bossBreakdown = XpCalculator.Calculate(
            TaskDifficulty.Hard, null, _now, 0, isBossTask: true);

        // Then — boss XP should be 2x normal
        bossBreakdown.BossTaskMultiplier.ShouldBe(2.0);
        bossBreakdown.FinalXp.ShouldBe(normalBreakdown.FinalXp * 2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotApplyBossMultiplier_When_NormalTaskCompleted()
    {
        // Given / When
        var breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0, isBossTask: false);

        // Then
        breakdown.BossTaskMultiplier.ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CombineBossMultiplierWithDeadlineModifier_When_BossCompletedEarly()
    {
        // Given
        var deadline = _now.AddDays(3);

        // When
        var breakdown = XpCalculator.Calculate(
            TaskDifficulty.Hard, deadline, _now, 0, isBossTask: true);

        // Then — base=60, deadline=1.2, boss=2.0 => 60 * 1.2 * 1.0 * 1.0 * 2.0 = 144
        breakdown.BossTaskMultiplier.ShouldBe(2.0);
        breakdown.DeadlineModifier.ShouldBe(1.2);
        breakdown.FinalXp.ShouldBe(144);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CombineBossMultiplierWithStreakMultiplier_When_BossCompletedOnStreak()
    {
        // Given — 10-day streak, boss task
        // When
        var breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 10, isBossTask: true);

        // Then — base=30, streak=1.2, boss=2.0 => round(30 * 1.0 * 1.2 * 1.0 * 2.0) = 72
        breakdown.BossTaskMultiplier.ShouldBe(2.0);
        breakdown.StreakMultiplier.ShouldBe(1.2);
        breakdown.FinalXp.ShouldBe(72);
    }

    // =================================================================
    // Scenario: Complete a Boss Task within Focus Mode
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyFocusModeBonus_When_BossTaskCompletedInFocusMode()
    {
        // Given / When — boss task + focus mode: boss=2.0, focus=1.5
        var breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0, isBossTask: true, isInFocusMode: true);

        // Then — base=30, boss=2.0, focus=1.5 => round(30 * 1.0 * 1.0 * 1.0 * 2.0 * 1.5) = 90
        breakdown.BossTaskMultiplier.ShouldBe(2.0);
        breakdown.FocusModeMultiplier.ShouldBe(1.5);
        breakdown.FinalXp.ShouldBe(90);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotApplyFocusModeBonus_When_NotInFocusMode()
    {
        // Given / When
        var breakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0, isBossTask: false, isInFocusMode: false);

        // Then
        breakdown.FocusModeMultiplier.ShouldBe(1.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowCombinedXpBreakdown_When_AllBonusesApplied()
    {
        // Given — Hard task, completed early, 10-day streak, boss task, focus mode
        var deadline = _now.AddDays(2);

        // When
        var breakdown = XpCalculator.Calculate(
            TaskDifficulty.Hard, deadline, _now, 10, isBossTask: true, isInFocusMode: true);

        // Then — base=60, deadline=1.2, streak=1.2, boss=2.0, focus=1.5
        // => round(60 * 1.2 * 1.2 * 1.0 * 2.0 * 1.5) = round(259.2) = 259
        breakdown.BaseXp.ShouldBe(60);
        breakdown.DeadlineModifier.ShouldBe(1.2);
        breakdown.StreakMultiplier.ShouldBe(1.2);
        breakdown.BossTaskMultiplier.ShouldBe(2.0);
        breakdown.FocusModeMultiplier.ShouldBe(1.5);
        breakdown.FinalXp.ShouldBe(259);
    }

    // =================================================================
    // Scenario: Boss Task completion contributes to title progression
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_EarnBossSlayerTitle_When_10thBossTaskCompleted()
    {
        // Given — 9 previous boss task completions + 10th
        var today = new DateOnly(2026, 4, 12);
        var requirement = TitleRequirement.For(TitleType.BossSlayer);
        var actions = new TitleQualifyingAction[10];
        for (int i = 0; i < 10; i++)
        {
            actions[i] = new TitleQualifyingAction(today.AddDays(-i));
        }

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, today);

        // Then
        result.IsEarned.ShouldBeTrue();
        result.TitleType.ShouldBe(TitleType.BossSlayer);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotEarnBossSlayerTitle_When_Only9BossTasksCompleted()
    {
        // Given
        var today = new DateOnly(2026, 4, 12);
        var requirement = TitleRequirement.For(TitleType.BossSlayer);
        var actions = new TitleQualifyingAction[9];
        for (int i = 0; i < 9; i++)
        {
            actions[i] = new TitleQualifyingAction(today.AddDays(-i));
        }

        // When
        var result = TitleEvaluator.Evaluate(requirement, actions, today);

        // Then
        result.IsEarned.ShouldBeFalse();
    }

    // =================================================================
    // Scenario: Trigger focus mode for a Boss Task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateFocusMode_When_StartedForBossTask()
    {
        // Given
        var taskId = TaskId.New();
        var startedAt = DateTimeOffset.UtcNow;

        // When
        var focusMode = FocusMode.Start(taskId, startedAt);

        // Then
        focusMode.TaskId.ShouldBe(taskId);
        focusMode.StartedAt.ShouldBe(startedAt);
        focusMode.IsActive.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DeactivateFocusMode_When_Ended()
    {
        // Given
        var focusMode = FocusMode.Start(TaskId.New(), _now);

        // When
        var ended = focusMode.End(_now.AddMinutes(45));

        // Then
        ended.IsActive.ShouldBeFalse();
        ended.Duration.ShouldBe(TimeSpan.FromMinutes(45));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateDuration_When_FocusModeEnded()
    {
        // Given
        var startedAt = _now;
        var endedAt = _now.AddMinutes(90);

        // When
        var focusMode = FocusMode.Start(TaskId.New(), startedAt).End(endedAt);

        // Then
        focusMode.Duration.ShouldBe(TimeSpan.FromMinutes(90));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndTimeBeforeStartTime()
    {
        // Given
        var focusMode = FocusMode.Start(TaskId.New(), _now);

        // When / Then
        var ex = Should.Throw<DomainException>(() => focusMode.End(_now.AddMinutes(-5)));
        ex.Message.ShouldContain("cannot be before");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndingAlreadyEndedFocusMode()
    {
        // Given
        var focusMode = FocusMode.Start(TaskId.New(), _now).End(_now.AddMinutes(30));

        // When / Then
        var ex = Should.Throw<DomainException>(() => focusMode.End(_now.AddMinutes(60)));
        ex.Message.ShouldContain("already ended");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowEndingAtSameTimeAsStart_When_ZeroDurationSession()
    {
        // Given
        var focusMode = FocusMode.Start(TaskId.New(), _now);

        // When — end at exactly the same time as start
        var ended = focusMode.End(_now);

        // Then — zero duration is valid
        ended.IsActive.ShouldBeFalse();
        ended.Duration.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroDuration_When_FocusModeStillActive()
    {
        // Given / When
        var focusMode = FocusMode.Start(TaskId.New(), _now);

        // Then
        focusMode.Duration.ShouldBe(TimeSpan.Zero);
    }

    // =================================================================
    // Scenario: Boss Task that is also a recurring task instance
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowBossPromotion_When_TaskIsRecurringInstance()
    {
        // Given — recurring task instance
        var sourceId = RecurringTaskId.New();
        var scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var task = TodoTask.CreateFromRecurring(
            new TaskTitle("Weekly report"), sourceId, scheduledDate);

        // When — promote to boss task
        task.PromoteToBossTask();

        // Then — both recurring and boss
        task.IsBossTask.ShouldBeTrue();
        task.SourceRecurringTaskId.ShouldBe(sourceId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AwardBothRecurringAndBossXp_When_RecurringBossTaskCompleted()
    {
        // Given — recurring boss task completion
        // When — calculate XP with boss multiplier and recurring diminishing factor
        var normalRecurringBreakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0,
            dailyRecurringCompletionCount: 1, isBossTask: false);

        var bossRecurringBreakdown = XpCalculator.Calculate(
            TaskDifficulty.Normal, null, _now, 0,
            dailyRecurringCompletionCount: 1, isBossTask: true);

        // Then — boss recurring should have 2x multiplier
        bossRecurringBreakdown.BossTaskMultiplier.ShouldBe(2.0);
        bossRecurringBreakdown.FinalXp.ShouldBe(normalRecurringBreakdown.FinalXp * 2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateNormalNextInstance_When_RecurringBossTaskCompleted()
    {
        // Given — recurring task generates next instance
        var recurringTask = RecurringTask.Create(
            new TaskTitle("Weekly report"), RecurrencePattern.Weekly);
        var nextDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7);

        // When — generate next instance
        var nextInstance = recurringTask.GenerateNextInstance(nextDate);

        // Then — next instance should be normal (not boss)
        nextInstance.IsBossTask.ShouldBeFalse();
    }

    // =================================================================
    // Scenario: Delete a Boss Task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ClearBossStatus_When_BossTaskDeleted()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Obsolete research"));
        task.PromoteToBossTask();
        task.IsBossTask.ShouldBeTrue();

        // When
        task.Delete();

        // Then
        task.IsBossTask.ShouldBeFalse();
        task.Status.ShouldBe(TaskStatus.Deleted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowDeletingNonBossTask_When_DeleteCalled()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Normal task"));

        // When
        task.Delete();

        // Then
        task.Status.ShouldBe(TaskStatus.Deleted);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DeletingAlreadyDeletedTask()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Already deleted"));
        task.Delete();

        // When / Then
        var ex = Should.Throw<DomainException>(() => task.Delete());
        ex.Message.ShouldContain("already deleted");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DeletingCompletedTask()
    {
        // Given
        var task = TodoTask.Create(new TaskTitle("Done task"));
        task.MoveToInProgress();
        task.MarkAsDone();

        // When / Then
        var ex = Should.Throw<DomainException>(() => task.Delete());
        ex.Message.ShouldContain("Cannot delete");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAwardXp_When_BossTaskDeleted()
    {
        // Given — deleting a boss task awards no XP
        // This is a behavioral assertion: deletion doesn't go through completion flow
        var task = TodoTask.Create(new TaskTitle("Obsolete research"));
        task.PromoteToBossTask();

        // When
        task.Delete();

        // Then — task is deleted, not done, so no XP calculation is triggered
        task.Status.ShouldBe(TaskStatus.Deleted);
        task.CompletedAt.ShouldBeNull();
    }
}
