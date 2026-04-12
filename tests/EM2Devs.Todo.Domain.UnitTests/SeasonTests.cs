using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for Season and SeasonalQuestLine.
/// Maps to: docs/features/progression/seasons.feature
/// Rule: "Seasons run quarterly and introduce themed content"
/// Rule: "Each season has a themed quest line"
/// </summary>
public sealed class SeasonTests
{
    private static readonly DateOnly _seasonStart = new(2026, 1, 1);
    private static readonly DateOnly _seasonEnd = new(2026, 3, 31);
    private static readonly DateOnly _midSeason = new(2026, 2, 15);

    // --- Season ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSeason_When_ValidParameters()
    {
        // Given / When
        var season = new Season("Season of the Architect", "Architecture", _seasonStart, _seasonEnd);

        // Then
        season.Name.ShouldBe("Season of the Architect");
        season.Theme.ShouldBe("Architecture");
        season.StartDate.ShouldBe(_seasonStart);
        season.EndDate.ShouldBe(_seasonEnd);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeActive_When_TodayIsWithinDateRange()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When / Then
        season.IsActive(_midSeason).ShouldBeTrue();
        season.IsActive(_seasonStart).ShouldBeTrue();
        season.IsActive(_seasonEnd).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeActive_When_TodayIsOutsideDateRange()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When / Then
        season.IsActive(_seasonStart.AddDays(-1)).ShouldBeFalse();
        season.IsActive(_seasonEnd.AddDays(1)).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnDaysRemaining_When_SeasonIsActive()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When
        int remaining = season.DaysRemaining(_midSeason);

        // Then
        remaining.ShouldBe(_seasonEnd.DayNumber - _midSeason.DayNumber);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroDaysRemaining_When_SeasonIsNotActive()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When
        int remaining = season.DaysRemaining(_seasonEnd.AddDays(1));

        // Then
        remaining.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportHasEnded_When_TodayIsAfterEndDate()
    {
        // Given
        var season = new Season("Test Season", "Theme", _seasonStart, _seasonEnd);

        // When / Then
        season.HasEnded(_seasonEnd.AddDays(1)).ShouldBeTrue();
        season.HasEnded(_seasonEnd).ShouldBeFalse();
        season.HasEnded(_midSeason).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NameIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("", "Theme", _seasonStart, _seasonEnd));
        ex.Message.ShouldContain("name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ThemeIsEmpty()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("Name", "", _seasonStart, _seasonEnd));
        ex.Message.ShouldContain("theme cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndDateBeforeStartDate()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("Name", "Theme", _seasonEnd, _seasonStart));
        ex.Message.ShouldContain("end date must be after start date");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EndDateEqualsStartDate()
    {
        // Given / When / Then
        var ex = Should.Throw<DomainException>(
            () => new Season("Name", "Theme", _seasonStart, _seasonStart));
        ex.Message.ShouldContain("end date must be after start date");
    }

    // --- SeasonalQuestLine ---

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

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AdvanceToNextStage_When_TaskThresholdReached()
    {
        // Given — stage 1 requires 3 tasks, completed 2
        var questLine = new SeasonalQuestLine(8, 1, 2);

        // When — complete 1 more (reaches threshold of 3)
        var result = questLine.RecordTaskCompletion(3);

        // Then — advance to stage 2
        result.CurrentStage.ShouldBe(2);
        result.TasksCompletedInStage.ShouldBe(0);
    }

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
}
