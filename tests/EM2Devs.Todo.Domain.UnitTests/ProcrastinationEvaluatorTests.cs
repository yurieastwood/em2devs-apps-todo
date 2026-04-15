using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ProcrastinationEvaluator domain service.
/// Covers all procrastination-detection.feature scenarios.
/// </summary>
public sealed class ProcrastinationEvaluatorTests
{
    // =================================================================
    // Scenario: Task rescheduled multiple times
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FlagAsProcrastinationCandidate_When_TaskRescheduled3OrMoreTimes()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Update resume"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldNotBeNull();
        candidate.Signals.ShouldContain(s => s.Type == ProcrastinationSignalType.RepeatedRescheduling);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeGentleInterventionPrompt_When_TaskFlaggedForRescheduling()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Update resume"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldNotBeNull();
        candidate.AvailableInterventions.ShouldNotBeEmpty();
        string message = ProcrastinationEvaluator.GenerateInterventionMessage(candidate);
        ProcrastinationEvaluator.IsSupportiveMessage(message).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsProcrastinationCandidate_When_TaskRescheduledLessThan3Times()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Quick task"));
        task.Reschedule();
        task.Reschedule();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldBeNull();
    }

    // =================================================================
    // Scenario: Task viewed repeatedly without action
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FlagAsProcrastinationCandidate_When_TaskViewed5TimesWithoutAction()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Call accountant"));
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldNotBeNull();
        candidate.Signals.ShouldContain(s => s.Type == ProcrastinationSignalType.RepeatedViewingWithoutAction);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagForViewingWithoutAction_When_TaskViewedLessThan5Times()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Quick look"));
        for (int i = 0; i < 4; i++)
        {
            task.RecordView();
        }

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagForViewingWithoutAction_When_TaskIsInProgress()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Started task"));
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        task.MoveToInProgress();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldBeNull();
    }

    // =================================================================
    // Scenario: Task viewed but marked as waiting on someone
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsProcrastinationCandidate_When_TaskHasWaitingReason()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Follow up with contractor"));
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        task.SetWaitingReason("Waiting on someone");

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then — should not be flagged because it is blocked by an external dependency
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FlagAgain_When_WaitingReasonCleared()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Follow up with contractor"));
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        task.SetWaitingReason("Waiting on someone");
        task.ClearWaitingReason();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldNotBeNull();
    }

    // =================================================================
    // Scenario: High-priority task consistently skipped
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FlagAsAvoided_When_HighPriorityTaskSkippedFor4Days()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Prepare investor pitch"),
            priority: TaskPriority.Critical);

        // When — 4 consecutive days in today view, 3 lower-priority tasks completed
        var candidate = ProcrastinationEvaluator.Evaluate(task,
            completedLowerPriorityTaskCount: 3,
            consecutiveDaysInTodayView: 4);

        // Then
        candidate.ShouldNotBeNull();
        candidate.Signals.ShouldContain(s => s.Type == ProcrastinationSignalType.HighPrioritySkipped);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsAvoided_When_LowPriorityTaskSkipped()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Organise bookshelf"),
            priority: TaskPriority.Low);

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task,
            completedLowerPriorityTaskCount: 3,
            consecutiveDaysInTodayView: 4);

        // Then
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsAvoided_When_NotEnoughDaysInTodayView()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Critical task"),
            priority: TaskPriority.Critical);

        // When — only 3 days, threshold is 4
        var candidate = ProcrastinationEvaluator.Evaluate(task,
            completedLowerPriorityTaskCount: 3,
            consecutiveDaysInTodayView: 3);

        // Then
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsAvoided_When_NotEnoughLowerPriorityTasksCompleted()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Critical task"),
            priority: TaskPriority.High);

        // When — only 2 lower-priority tasks completed, threshold is 3
        var candidate = ProcrastinationEvaluator.Evaluate(task,
            completedLowerPriorityTaskCount: 2,
            consecutiveDaysInTodayView: 4);

        // Then
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsAvoided_When_HighPriorityTaskIsInProgress()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Started critical task"),
            priority: TaskPriority.Critical);
        task.MoveToInProgress();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task,
            completedLowerPriorityTaskCount: 3,
            consecutiveDaysInTodayView: 4);

        // Then
        candidate.ShouldBeNull();
    }

    // =================================================================
    // Scenario: Multiple signals increase urgency
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveHigherUrgency_When_MultipleSignalsDetected()
    {
        // Given — task with multiple signals
        var multiSignalTask = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Complete tax return"),
            priority: TaskPriority.High);
        multiSignalTask.Reschedule();
        multiSignalTask.Reschedule();
        multiSignalTask.Reschedule();
        for (int i = 0; i < 5; i++)
        {
            multiSignalTask.RecordView();
        }

        // Given — task with single signal
        var singleSignalTask = TodoTask.Create(TestData.TestUserId, new TaskTitle("Simple rescheduled task"));
        singleSignalTask.Reschedule();
        singleSignalTask.Reschedule();
        singleSignalTask.Reschedule();

        // When
        var multiCandidate = ProcrastinationEvaluator.Evaluate(multiSignalTask,
            completedLowerPriorityTaskCount: 3,
            consecutiveDaysInTodayView: 4);
        var singleCandidate = ProcrastinationEvaluator.Evaluate(singleSignalTask);

        // Then
        multiCandidate.ShouldNotBeNull();
        singleCandidate.ShouldNotBeNull();
        multiCandidate.UrgencyScore.ShouldBeGreaterThan(singleCandidate.UrgencyScore);
        multiCandidate.Signals.Count.ShouldBeGreaterThan(singleCandidate.Signals.Count);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SumSignalWeights_When_CalculatingUrgencyScore()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Multi-signal task"),
            priority: TaskPriority.High);
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        for (int i = 0; i < 5; i++)
        {
            task.RecordView();
        }

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then — reschedule weight (2) + view without action weight (1) = 3
        candidate.ShouldNotBeNull();
        candidate.UrgencyScore.ShouldBe(3);
    }

    // =================================================================
    // Scenario: Task open well past due date
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FlagAsProcrastinationCandidate_When_TaskOverdueBy7OrMoreDays()
    {
        // Given — task due 10 days ago
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("File insurance claim"),
            dueDate: DateTimeOffset.UtcNow.AddDays(-10));

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldNotBeNull();
        candidate.Signals.ShouldContain(s => s.Type == ProcrastinationSignalType.OverduePastThreshold);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_FlagAsProcrastinationCandidate_When_TaskOverdueByExactly7Days()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Exactly 7 days overdue"),
            dueDate: DateTimeOffset.UtcNow.AddDays(-7));

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldNotBeNull();
        candidate.Signals.ShouldContain(s => s.Type == ProcrastinationSignalType.OverduePastThreshold);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsOverdue_When_TaskOverdueByLessThan7Days()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Recently overdue"),
            dueDate: DateTimeOffset.UtcNow.AddDays(-6));

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotFlagAsOverdue_When_TaskHasNoDueDate()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("No due date task"));

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldBeNull();
    }

    // =================================================================
    // Scenario: View intervention options for a procrastinated task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OfferAllInterventionOptions_When_TaskFlaggedForProcrastination()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Update resume"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldNotBeNull();
        candidate.AvailableInterventions.Count.ShouldBe(5);
        candidate.AvailableInterventions.ShouldContain(i => i.Type == InterventionOptionType.BreakItDown);
        candidate.AvailableInterventions.ShouldContain(i => i.Type == InterventionOptionType.Delegate);
        candidate.AvailableInterventions.ShouldContain(i => i.Type == InterventionOptionType.ReEvaluate);
        candidate.AvailableInterventions.ShouldContain(i => i.Type == InterventionOptionType.BossTaskIt);
        candidate.AvailableInterventions.ShouldContain(i => i.Type == InterventionOptionType.RescheduleWithIntent);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ProvideDescriptiveMessages_When_ListingInterventionOptions()
    {
        // Given / When
        var interventions = ProcrastinationEvaluator.BuildInterventions();

        // Then
        foreach (var intervention in interventions)
        {
            intervention.SupportiveMessage.ShouldNotBeNullOrWhiteSpace();
            ProcrastinationEvaluator.IsSupportiveMessage(intervention.SupportiveMessage).ShouldBeTrue();
        }
    }

    // =================================================================
    // Scenario: Break down a procrastinated task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SuggestSubtasks_When_BreakingDownProcrastinatedTask()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Prepare investor pitch"));

        // When
        var subtasks = ProcrastinationEvaluator.SuggestSubtasks(task);

        // Then
        subtasks.Count.ShouldBe(4);
        subtasks.ShouldAllBe(s => s.Contains(task.Title.Value));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_SuggestingSubtasksForNull()
    {
        Should.Throw<ArgumentNullException>(() => ProcrastinationEvaluator.SuggestSubtasks(null!));
    }

    // =================================================================
    // Scenario: Re-evaluate a procrastinated task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ProvideReEvaluationPrompts_When_ChoosingReEvaluate()
    {
        // When
        var prompts = ProcrastinationEvaluator.GetReEvaluationPrompts();

        // Then
        prompts.Count.ShouldBe(4);
        prompts.ShouldContain("Does this still need to happen?");
        prompts.ShouldContain("What would the consequence be if you never did this?");
        prompts.ShouldContain("Is someone else depending on this?");
        prompts.ShouldContain("Would you add this task today if it were not already here?");
    }

    // =================================================================
    // Scenario: Promote procrastinated task to Boss Task
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PromoteToBossTask_When_ChoosingBossTaskIntervention()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Write thesis chapter"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // When
        task.PromoteToBossTask();

        // Then
        task.IsBossTask.ShouldBeTrue();
        candidate.ShouldNotBeNull();
        candidate.AvailableInterventions.ShouldContain(i => i.Type == InterventionOptionType.BossTaskIt);
    }

    // =================================================================
    // Scenario: Completing a Boss Task clears procrastination signals
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ClearProcrastinationSignals_When_BossTaskCompleted()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Write thesis chapter"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        ProcrastinationEvaluator.Evaluate(task);
        task.ProcrastinationSignals.Count.ShouldBeGreaterThan(0);
        task.PromoteToBossTask();

        // When — complete the boss task
        task.MoveToInProgress();
        task.MarkAsDone();
        task.ClearProcrastinationSignals();

        // Then
        task.ProcrastinationSignals.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAppearInProcrastinationInsights_When_CompletedBossTask()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Write thesis chapter"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        ProcrastinationEvaluator.Evaluate(task);
        task.PromoteToBossTask();
        task.MoveToInProgress();
        task.MarkAsDone();
        task.ClearProcrastinationSignals();

        // When — re-evaluate
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then — done tasks are not flagged
        candidate.ShouldBeNull();
    }

    // =================================================================
    // Scenario: Reschedule with commitment note
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AttachCommitmentNote_When_ReschedulingWithIntent()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Schedule dentist appointment"));
        var note = new CommitmentNote("Will call during lunch break");

        // When
        task.RescheduleWithCommitment(note);

        // Then
        task.RescheduleCount.ShouldBe(1);
        task.CommitmentNote.ShouldNotBeNull();
        task.CommitmentNote.Text.ShouldBe("Will call during lunch break");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CommitmentNoteTextIsEmpty()
    {
        Should.Throw<DomainException>(() => new CommitmentNote(""));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CommitmentNoteTextIsWhitespace()
    {
        Should.Throw<DomainException>(() => new CommitmentNote("   "));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ReschedulingWithNullNote()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test"));
        Should.Throw<ArgumentNullException>(() => task.RescheduleWithCommitment(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ReschedulingCompletedTaskWithCommitment()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Done task"));
        task.MoveToInProgress();
        task.MarkAsDone();

        // When / Then
        var note = new CommitmentNote("Test note");
        Should.Throw<DomainException>(() => task.RescheduleWithCommitment(note));
    }

    // =================================================================
    // Scenario: View procrastination patterns
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowProcrastinationPatterns_When_AnalysingTaskHistory()
    {
        // Given — tasks with procrastination signals
        var task1 = TodoTask.Create(TestData.TestUserId, new TaskTitle("Task one"));
        task1.Reschedule();
        task1.Reschedule();
        task1.Reschedule();
        ProcrastinationEvaluator.Evaluate(task1);

        var task2 = TodoTask.Create(TestData.TestUserId, new TaskTitle("Task two"));
        task2.Reschedule();
        task2.Reschedule();
        task2.Reschedule();
        ProcrastinationEvaluator.Evaluate(task2);

        var task3 = TodoTask.Create(TestData.TestUserId, new TaskTitle("Task three"));
        for (int i = 0; i < 5; i++)
        {
            task3.RecordView();
        }

        ProcrastinationEvaluator.Evaluate(task3);

        List<TodoTask> allTasks = [task1, task2, task3];
        var completedAfterIntervention = new List<TodoTask> { task1 };

        // When
        var patterns = ProcrastinationEvaluator.AnalysePatterns(
            allTasks,
            completedAfterIntervention);

        // Then
        patterns.TotalProcrastinatedTasks.ShouldBe(3);
        patterns.InterventionSuccessRate.ShouldBeGreaterThan(0);
        patterns.MostCommonSignalType.ShouldBe(ProcrastinationSignalType.RepeatedRescheduling);

        // Guard — null input must be rejected with correct parameter name
        ArgumentNullException nullEx = Should.Throw<ArgumentNullException>(() =>
            ProcrastinationEvaluator.AnalysePatterns(null!, completedAfterIntervention));
        nullEx.ParamName.ShouldBe("allTasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowZeroSuccessRate_When_NoProcrastinatedTasks()
    {
        // When
        var patterns = ProcrastinationEvaluator.AnalysePatterns([], []);

        // Then
        patterns.TotalProcrastinatedTasks.ShouldBe(0);
        patterns.InterventionSuccessRate.ShouldBe(0);
        patterns.MostCommonSignalType.ShouldBeNull();
    }

    // =================================================================
    // Scenario: Intervention tone is always supportive
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeAcknowledgementOfDifficulty_When_PresentingIntervention()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Overwhelming task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // When
        string message = ProcrastinationEvaluator.GenerateInterventionMessage(candidate!);

        // Then
        message.ShouldContain("This task can feel overwhelming");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeConstructiveSuggestion_When_PresentingIntervention()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Hard task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // When
        string message = ProcrastinationEvaluator.GenerateInterventionMessage(candidate!);

        // Then
        message.ShouldContain("Try starting with just the first step");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotContainShamingWords_When_PresentingIntervention()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Shameful task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // When
        string message = ProcrastinationEvaluator.GenerateInterventionMessage(candidate!);

        // Then
        message.ShouldNotContain("failure");
        message.ShouldNotContain("lazy");
        message.ShouldNotContain("behind");
        message.ShouldNotContain("overdue guilt");
        ProcrastinationEvaluator.IsSupportiveMessage(message).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotContainShamingWords_InAllInterventionMessages()
    {
        // Given / When
        var interventions = ProcrastinationEvaluator.BuildInterventions();

        // Then
        foreach (var intervention in interventions)
        {
            intervention.SupportiveMessage.ShouldNotContain("failure");
            intervention.SupportiveMessage.ShouldNotContain("lazy");
            intervention.SupportiveMessage.ShouldNotContain("behind");
            intervention.SupportiveMessage.ShouldNotContain("overdue guilt");
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_MessageContainsShamingWord()
    {
        ProcrastinationEvaluator.IsSupportiveMessage("You are a failure").ShouldBeFalse();
        ProcrastinationEvaluator.IsSupportiveMessage("Don't be lazy").ShouldBeFalse();
        ProcrastinationEvaluator.IsSupportiveMessage("You are behind schedule").ShouldBeFalse();
        ProcrastinationEvaluator.IsSupportiveMessage("overdue guilt is bad").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_MessageIsEmptyOrWhitespace()
    {
        ProcrastinationEvaluator.IsSupportiveMessage("").ShouldBeFalse();
        ProcrastinationEvaluator.IsSupportiveMessage("   ").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_MessageIsSupportive()
    {
        ProcrastinationEvaluator.IsSupportiveMessage("You are doing great, keep going!").ShouldBeTrue();
    }

    // =================================================================
    // Edge cases and guard clauses
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_EvaluatingNull()
    {
        Should.Throw<ArgumentNullException>(() => ProcrastinationEvaluator.Evaluate(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_DetectingSignalsForNull()
    {
        Should.Throw<ArgumentNullException>(() => ProcrastinationEvaluator.DetectSignals(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_GeneratingMessageForNull()
    {
        Should.Throw<ArgumentNullException>(() => ProcrastinationEvaluator.GenerateInterventionMessage(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AnalysingPatternsWithNullTasks()
    {
        // Given — null allTasks parameter
        IReadOnlyList<TodoTask>? nullTasks = null;
        var emptyCompleted = Array.Empty<TodoTask>() as IReadOnlyList<TodoTask>;

        // When / Then — must be ArgumentNullException with the correct parameter name
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(
            () => ProcrastinationEvaluator.AnalysePatterns(nullTasks!, emptyCompleted));
        ex.ParamName.ShouldBe("allTasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AnalysingPatternsWithNullCompleted()
    {
        Should.Throw<ArgumentNullException>(() => ProcrastinationEvaluator.AnalysePatterns([], null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_TaskIsDone()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Done task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        task.MoveToInProgress();
        task.MarkAsDone();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNull_When_TaskIsSkipped()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Skipped task"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        task.Skip();

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // Then
        candidate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeForwardPathInMessage_When_GeneratingIntervention()
    {
        // Given
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test message parts"));
        task.Reschedule();
        task.Reschedule();
        task.Reschedule();
        var candidate = ProcrastinationEvaluator.Evaluate(task);

        // When
        string message = ProcrastinationEvaluator.GenerateInterventionMessage(candidate!);

        // Then — verify all three parts of the message
        message.ShouldContain("You have several options to help you move forward");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeTasksWithNoSignals_When_AnalysingPatterns()
    {
        // Given — mix of tasks: some with signals, some without
        var taskWithSignals = TodoTask.Create(TestData.TestUserId, new TaskTitle("Has signals"));
        taskWithSignals.Reschedule();
        taskWithSignals.Reschedule();
        taskWithSignals.Reschedule();
        ProcrastinationEvaluator.Evaluate(taskWithSignals);

        var taskWithoutSignals = TodoTask.Create(TestData.TestUserId, new TaskTitle("No signals"));

        // When
        var patterns = ProcrastinationEvaluator.AnalysePatterns(
            [taskWithSignals, taskWithoutSignals], []);

        // Then — only the task with signals counts
        patterns.TotalProcrastinatedTasks.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateCorrectSuccessRate_When_AnalysingPatterns()
    {
        // Given — 2 procrastinated tasks, 1 completed after intervention
        var task1 = TodoTask.Create(TestData.TestUserId, new TaskTitle("Task one"));
        task1.Reschedule();
        task1.Reschedule();
        task1.Reschedule();
        ProcrastinationEvaluator.Evaluate(task1);

        var task2 = TodoTask.Create(TestData.TestUserId, new TaskTitle("Task two"));
        task2.Reschedule();
        task2.Reschedule();
        task2.Reschedule();
        ProcrastinationEvaluator.Evaluate(task2);

        // When — 1 out of 2 = 50%
        var patterns = ProcrastinationEvaluator.AnalysePatterns(
            [task1, task2], [task1]);

        // Then
        patterns.InterventionSuccessRate.ShouldBe(50.0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseEvaluationDate_When_CheckingOverdue()
    {
        // Given — task due 10 days before evaluation date
        var evaluationDate = new DateTimeOffset(2026, 4, 12, 0, 0, 0, TimeSpan.Zero);
        var task = TodoTask.Create(TestData.TestUserId,
            new TaskTitle("Test with evaluation date"),
            dueDate: evaluationDate.AddDays(-10));

        // When
        var candidate = ProcrastinationEvaluator.Evaluate(task, evaluationDate: evaluationDate);

        // Then
        candidate.ShouldNotBeNull();
        candidate.Signals.ShouldContain(s => s.Type == ProcrastinationSignalType.OverduePastThreshold);
    }
}
