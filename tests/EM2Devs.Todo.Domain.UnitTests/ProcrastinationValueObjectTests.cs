using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Tests for procrastination-related value objects: ProcrastinationSignal, InterventionOption,
/// CommitmentNote, and ProcrastinationCandidate.
/// </summary>
public sealed class ProcrastinationValueObjectTests
{
    // =================================================================
    // ProcrastinationSignal
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSignal_When_ValidTypeAndWeight()
    {
        // When
        var signal = new ProcrastinationSignal(ProcrastinationSignalType.RepeatedRescheduling, 2);

        // Then
        signal.Type.ShouldBe(ProcrastinationSignalType.RepeatedRescheduling);
        signal.Weight.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SignalWeightIsZero()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ProcrastinationSignal(ProcrastinationSignalType.RepeatedRescheduling, 0));
        ex.Message.ShouldContain("positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SignalWeightIsNegative()
    {
        var ex = Should.Throw<DomainException>(() =>
            new ProcrastinationSignal(ProcrastinationSignalType.RepeatedRescheduling, -1));
        ex.Message.ShouldContain("positive");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_SameTypeAndWeight()
    {
        var signal1 = new ProcrastinationSignal(ProcrastinationSignalType.HighPrioritySkipped, 3);
        var signal2 = new ProcrastinationSignal(ProcrastinationSignalType.HighPrioritySkipped, 3);
        signal1.ShouldBe(signal2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentType()
    {
        var signal1 = new ProcrastinationSignal(ProcrastinationSignalType.HighPrioritySkipped, 3);
        var signal2 = new ProcrastinationSignal(ProcrastinationSignalType.OverduePastThreshold, 3);
        signal1.ShouldNotBe(signal2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentWeight()
    {
        var signal1 = new ProcrastinationSignal(ProcrastinationSignalType.HighPrioritySkipped, 3);
        var signal2 = new ProcrastinationSignal(ProcrastinationSignalType.HighPrioritySkipped, 5);
        signal1.ShouldNotBe(signal2);
    }

    // =================================================================
    // InterventionOption
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateInterventionOption_When_ValidParameters()
    {
        // When
        var option = new InterventionOption(InterventionOptionType.BreakItDown,
            "Try breaking it into smaller steps.");

        // Then
        option.Type.ShouldBe(InterventionOptionType.BreakItDown);
        option.SupportiveMessage.ShouldBe("Try breaking it into smaller steps.");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InterventionMessageIsEmpty()
    {
        var ex = Should.Throw<DomainException>(() =>
            new InterventionOption(InterventionOptionType.BreakItDown, ""));
        ex.Message.ShouldContain("empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InterventionMessageIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(() =>
            new InterventionOption(InterventionOptionType.BreakItDown, "   "));
        ex.Message.ShouldContain("empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeEqual_When_SameInterventionTypeAndMessage()
    {
        var option1 = new InterventionOption(InterventionOptionType.Delegate, "Share the load.");
        var option2 = new InterventionOption(InterventionOptionType.Delegate, "Share the load.");
        option1.ShouldBe(option2);
    }

    // =================================================================
    // CommitmentNote
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCommitmentNote_When_ValidText()
    {
        // When
        var note = new CommitmentNote("Will call during lunch break");

        // Then
        note.Text.ShouldBe("Will call during lunch break");
        note.CreatedAt.ShouldBeGreaterThan(DateTimeOffset.MinValue);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseProvidedCreatedAt_When_Specified()
    {
        var customDate = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);
        var note = new CommitmentNote("Test", customDate);
        note.CreatedAt.ShouldBe(customDate);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CommitmentNoteTextIsNull()
    {
        var ex = Should.Throw<DomainException>(() => new CommitmentNote(null!));
        ex.Message.ShouldContain("empty");
    }

    // =================================================================
    // ProcrastinationCandidate
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCandidate_When_ValidParameters()
    {
        // Given
        var taskId = TaskId.New();
        var signals = new List<ProcrastinationSignal>
        {
            new(ProcrastinationSignalType.RepeatedRescheduling, 2)
        };
        var interventions = new List<InterventionOption>
        {
            new(InterventionOptionType.BreakItDown, "Try smaller steps.")
        };

        // When
        var candidate = new ProcrastinationCandidate(taskId, signals, interventions);

        // Then
        candidate.TaskId.ShouldBe(taskId);
        candidate.Signals.Count.ShouldBe(1);
        candidate.UrgencyScore.ShouldBe(2);
        candidate.AvailableInterventions.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CandidateHasNoSignals()
    {
        var taskId = TaskId.New();
        var ex = Should.Throw<DomainException>(() =>
            new ProcrastinationCandidate(taskId, [], []));
        ex.Message.ShouldContain("at least one signal");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CandidateTaskIdIsNull()
    {
        var signals = new List<ProcrastinationSignal>
        {
            new(ProcrastinationSignalType.RepeatedRescheduling, 2)
        };
        Should.Throw<ArgumentNullException>(() =>
            new ProcrastinationCandidate(null!, signals, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CandidateSignalsIsNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ProcrastinationCandidate(TaskId.New(), null!, []));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CandidateInterventionsIsNull()
    {
        var signals = new List<ProcrastinationSignal>
        {
            new(ProcrastinationSignalType.RepeatedRescheduling, 2)
        };
        Should.Throw<ArgumentNullException>(() =>
            new ProcrastinationCandidate(TaskId.New(), signals, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SumWeights_When_MultipleSignals()
    {
        var signals = new List<ProcrastinationSignal>
        {
            new(ProcrastinationSignalType.RepeatedRescheduling, 2),
            new(ProcrastinationSignalType.RepeatedViewingWithoutAction, 1),
            new(ProcrastinationSignalType.HighPrioritySkipped, 3)
        };

        var candidate = new ProcrastinationCandidate(TaskId.New(), signals, []);
        candidate.UrgencyScore.ShouldBe(6);
    }

    // =================================================================
    // TodoTask procrastination-related methods
    // =================================================================

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetWaitingReason_When_ReasonProvided()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Waiting task"));
        task.SetWaitingReason("Waiting on someone");
        task.WaitingReason.ShouldBe("Waiting on someone");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WaitingReasonIsEmpty()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test"));
        DomainException ex = Should.Throw<DomainException>(() => task.SetWaitingReason(""));
        ex.Message.ShouldContain("empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WaitingReasonIsWhitespace()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test"));
        Should.Throw<DomainException>(() => task.SetWaitingReason("   "));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ClearWaitingReason_When_Called()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test"));
        task.SetWaitingReason("Waiting");
        task.ClearWaitingReason();
        task.WaitingReason.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddProcrastinationSignal_When_SignalProvided()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test"));
        var signal = new ProcrastinationSignal(ProcrastinationSignalType.RepeatedRescheduling, 2);
        task.AddProcrastinationSignal(signal);
        task.ProcrastinationSignals.Count.ShouldBe(1);
        task.ProcrastinationSignals[0].ShouldBe(signal);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullSignal()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test"));
        Should.Throw<ArgumentNullException>(() => task.AddProcrastinationSignal(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ClearAllSignals_When_ClearProcrastinationSignalsCalled()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("Test"));
        task.AddProcrastinationSignal(new ProcrastinationSignal(ProcrastinationSignalType.RepeatedRescheduling, 2));
        task.AddProcrastinationSignal(new ProcrastinationSignal(ProcrastinationSignalType.OverduePastThreshold, 2));
        task.ProcrastinationSignals.Count.ShouldBe(2);
        task.ClearProcrastinationSignals();
        task.ProcrastinationSignals.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullWaitingReason_When_TaskCreated()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("New task"));
        task.WaitingReason.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullCommitmentNote_When_TaskCreated()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("New task"));
        task.CommitmentNote.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveEmptyProcrastinationSignals_When_TaskCreated()
    {
        var task = TodoTask.Create(TestData.TestUserId, new TaskTitle("New task"));
        task.ProcrastinationSignals.ShouldBeEmpty();
    }
}
