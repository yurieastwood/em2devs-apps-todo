using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for SeasonalQuestLine.
/// Maps to: docs/features/progression/seasons.feature
/// Rule: "Each season has a themed quest line that provides guided challenges"
/// </summary>
public sealed class SeasonalQuestLineTests
{
    // --- Scenario: Start the seasonal quest line ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartAtStageOne_When_QuestLineCreated()
    {
        // Given / When
        var questLine = SeasonalQuestLine.Start(8);

        // Then
        questLine.TotalStages.ShouldBe(8);
        questLine.CurrentStage.ShouldBe(1);
        questLine.TasksCompletedInStage.ShouldBe(0);
        questLine.IsCompleted.ShouldBeFalse();
        questLine.IsLocked.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowStage1Available_When_QuestLineStarted()
    {
        // Given
        var questLine = SeasonalQuestLine.Start(8);

        // When / Then — stage 1 available, stages 2-8 locked
        questLine.IsStageAvailable(1).ShouldBeTrue();
        for (int i = 2; i <= 8; i++)
        {
            questLine.IsStageAvailable(i).ShouldBeFalse();
        }
    }

    // --- Scenario Outline: Complete a seasonal quest line stage ---

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData(1, 3, 2, 2)]
    [InlineData(3, 5, 4, 4)]
    [InlineData(5, 7, 6, 6)]
    public void Should_AdvanceToNextStage_When_TaskThresholdReached(
        int stage, int required, int completed, int nextStage)
    {
        // Given — on stage with completed tasks (one less than required)
        var questLine = new SeasonalQuestLine(8, stage, completed);

        // When — complete another qualifying task
        var result = questLine.RecordTaskCompletion(required);

        // Then — advance to next stage
        result.CurrentStage.ShouldBe(nextStage);
        result.TasksCompletedInStage.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncrementTaskCount_When_RecordingCompletion()
    {
        // Given
        var questLine = SeasonalQuestLine.Start(8);

        // When
        var result = questLine.RecordTaskCompletion(3);

        // Then
        result.TasksCompletedInStage.ShouldBe(1);
        result.CurrentStage.ShouldBe(1);
    }

    // --- Scenario: Complete the full seasonal quest line ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MarkCompleted_When_FinalStageCompleted()
    {
        // Given — on last stage (8), completed 6 of 7 required
        var questLine = new SeasonalQuestLine(8, 8, 6);

        // When — complete final task
        var result = questLine.RecordTaskCompletion(7);

        // Then — quest line completed
        result.IsCompleted.ShouldBeTrue();
        result.CurrentStage.ShouldBe(9); // past total stages
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NoOp_When_RecordingCompletionOnFinishedQuestLine()
    {
        // Given — already completed
        var questLine = new SeasonalQuestLine(8, 9, 0);

        // When
        var result = questLine.RecordTaskCompletion(5);

        // Then — unchanged
        result.IsCompleted.ShouldBeTrue();
        result.CurrentStage.ShouldBe(9);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrue_When_StageIsCurrentAndAvailable()
    {
        // Given
        var questLine = new SeasonalQuestLine(8, 3, 0);

        // When / Then
        questLine.IsStageAvailable(3).ShouldBeTrue();
        questLine.IsStageAvailable(2).ShouldBeFalse();
        questLine.IsStageAvailable(4).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTasksRemaining_When_StageInProgress()
    {
        // Given — stage requires 5 tasks, completed 2
        var questLine = new SeasonalQuestLine(8, 3, 2);

        // When
        int remaining = questLine.TasksRemainingInStage(5);

        // Then
        remaining.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroTasksRemaining_When_Completed()
    {
        // Given — completed
        var questLine = new SeasonalQuestLine(8, 9, 0);

        // When
        int remaining = questLine.TasksRemainingInStage(5);

        // Then
        remaining.ShouldBe(0);
    }

    // --- Validation ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TotalStagesIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => SeasonalQuestLine.Start(0));
        ex.Message.ShouldContain("must be between 1 and");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TotalStagesExceedsMax()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => SeasonalQuestLine.Start(SeasonalQuestLine.MaxStages + 1));
        ex.Message.ShouldContain("must be between 1 and");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CurrentStageIsZero()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new SeasonalQuestLine(8, 0, 0));
        ex.Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TasksCompletedIsNegative()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(() => new SeasonalQuestLine(8, 1, -1));
        ex.Message.ShouldContain("cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSingleStageQuestLine_When_TotalStagesIsOne()
    {
        // Given / When
        var questLine = SeasonalQuestLine.Start(1);

        // Then
        questLine.TotalStages.ShouldBe(1);
        questLine.CurrentStage.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompleteAndStayIdempotent_When_RecordingOnCompletedQuestLine()
    {
        // Given — completed single-stage quest line
        var questLine = SeasonalQuestLine.Start(1).RecordTaskCompletion(1);
        questLine.IsCompleted.ShouldBeTrue();

        // When — record more completions
        var result = questLine.RecordTaskCompletion(1);

        // Then — still completed, no state change
        result.IsCompleted.ShouldBeTrue();
        result.CurrentStage.ShouldBe(questLine.CurrentStage);
        result.TasksCompletedInStage.ShouldBe(questLine.TasksCompletedInStage);
    }

    // --- Scenario: Seamless transition — locking incomplete quest line ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LockQuestLine_When_SeasonEnds()
    {
        // Given — in progress on stage 3
        var questLine = new SeasonalQuestLine(8, 3, 2);

        // When — season ends, quest line locked
        var locked = questLine.Lock();

        // Then
        locked.IsLocked.ShouldBeTrue();
        locked.CurrentStage.ShouldBe(3);
        locked.TasksCompletedInStage.ShouldBe(2);
        locked.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotAdvance_When_QuestLineIsLocked()
    {
        // Given — locked quest line
        var questLine = new SeasonalQuestLine(8, 3, 2).Lock();

        // When — try to record completion
        var result = questLine.RecordTaskCompletion(3);

        // Then — no change
        result.CurrentStage.ShouldBe(3);
        result.TasksCompletedInStage.ShouldBe(2);
        result.IsLocked.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotShowStageAvailable_When_QuestLineIsLocked()
    {
        // Given — locked quest line
        var questLine = new SeasonalQuestLine(8, 3, 2).Lock();

        // When / Then
        questLine.IsStageAvailable(3).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroTasksRemaining_When_QuestLineIsLocked()
    {
        // Given — locked quest line
        var questLine = new SeasonalQuestLine(8, 3, 2).Lock();

        // When
        int remaining = questLine.TasksRemainingInStage(5);

        // Then
        remaining.ShouldBe(0);
    }
}
