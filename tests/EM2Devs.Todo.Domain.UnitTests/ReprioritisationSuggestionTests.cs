using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ReprioritisationSuggestion: reprioritisation assistance offered.
/// </summary>
public sealed class ReprioritisationSuggestionTests
{
    private static TaskId NewId() => TaskId.New();

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepTasksWithinCapacity_AndDeferTheRest()
    {
        // Given — 12 tasks with 1 unit each, capacity 6
        List<(TaskId, int)> ordered = new List<(TaskId, int)>();
        for (int i = 0; i < 12; i++)
        {
            ordered.Add((NewId(), 1));
        }

        DateOnly nextDay = new DateOnly(2026, 4, 14);
        ReprioritisationSuggestion suggestion = ReprioritisationSuggestion.Build(
            DayOfWeek.Monday, 6, ordered, nextDay);

        suggestion.Suggestions.Count.ShouldBe(12);
        suggestion.Suggestions.Take(6).ShouldAllBe(s => s.Action == ReprioritisationAction.Keep);
        suggestion.Suggestions.Skip(6).ShouldAllBe(s => s.Action == ReprioritisationAction.Defer);
        suggestion.Suggestions.Skip(6).ShouldAllBe(s => s.DeferTo == nextDay);
        suggestion.Capacity.ShouldBe(6);
        suggestion.ScheduledUnits.ShouldBe(12);
        suggestion.Day.ShouldBe(DayOfWeek.Monday);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DeferHardTasksFirst_WhenUnitsExceedCapacity()
    {
        // Given — three tasks of 2 units each (Hard), capacity 4 -> keep first two, defer third.
        (TaskId Id, int Units) a = (NewId(), 2);
        (TaskId Id, int Units) b = (NewId(), 2);
        (TaskId Id, int Units) c = (NewId(), 2);
        List<(TaskId, int)> ordered = new List<(TaskId, int)> { a, b, c };

        ReprioritisationSuggestion suggestion = ReprioritisationSuggestion.Build(
            DayOfWeek.Tuesday, 4, ordered, new DateOnly(2026, 4, 15));

        suggestion.Suggestions[0].Action.ShouldBe(ReprioritisationAction.Keep);
        suggestion.Suggestions[1].Action.ShouldBe(ReprioritisationAction.Keep);
        suggestion.Suggestions[2].Action.ShouldBe(ReprioritisationAction.Defer);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_KeepAllTasks_When_WithinCapacity()
    {
        List<(TaskId, int)> ordered = new List<(TaskId, int)>
        {
            (NewId(), 1), (NewId(), 1), (NewId(), 1),
        };
        ReprioritisationSuggestion suggestion = ReprioritisationSuggestion.Build(
            DayOfWeek.Wednesday, 6, ordered, new DateOnly(2026, 4, 15));

        suggestion.Suggestions.ShouldAllBe(s => s.Action == ReprioritisationAction.Keep);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_OrderedTasksNull()
    {
        ArgumentNullException ex = Should.Throw<ArgumentNullException>(() => ReprioritisationSuggestion.Build(
            DayOfWeek.Monday, 6, null!, new DateOnly(2026, 4, 14)));
        ex.ParamName.ShouldBe("orderedTasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_CapacityNegative()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReprioritisationSuggestion.Build(
            DayOfWeek.Monday, -1, new List<(TaskId, int)>(), new DateOnly(2026, 4, 14)));
        ex.Message.ShouldContain("Capacity cannot be negative");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptCapacityOfZero_AndDeferAllTasks()
    {
        // Kills `capacity < 0` -> `capacity <= 0` mutation.
        List<(TaskId, int)> tasks = new List<(TaskId, int)> { (NewId(), 1) };
        ReprioritisationSuggestion suggestion = ReprioritisationSuggestion.Build(
            DayOfWeek.Monday, 0, tasks, new DateOnly(2026, 4, 14));
        suggestion.Suggestions.Single().Action.ShouldBe(ReprioritisationAction.Defer);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowConstruction_When_DeferredWithoutDate()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new ReprioritisationTaskSuggestion(NewId(), ReprioritisationAction.Defer, null));
        ex.Message.ShouldContain("defer-to date");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowConstruction_When_KeptWithDate()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new ReprioritisationTaskSuggestion(NewId(), ReprioritisationAction.Keep, new DateOnly(2026, 4, 14)));
        ex.Message.ShouldContain("must not specify");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowConstruction_When_TaskIdNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            new ReprioritisationTaskSuggestion(null!, ReprioritisationAction.Keep, null));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_PreserveOrderFromInput()
    {
        TaskId firstId = NewId();
        TaskId secondId = NewId();
        List<(TaskId, int)> ordered = new List<(TaskId, int)>
        {
            (firstId, 1), (secondId, 1),
        };
        ReprioritisationSuggestion suggestion = ReprioritisationSuggestion.Build(
            DayOfWeek.Thursday, 10, ordered, new DateOnly(2026, 4, 16));

        suggestion.Suggestions[0].TaskId.ShouldBe(firstId);
        suggestion.Suggestions[1].TaskId.ShouldBe(secondId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptEmptyTasks_AndReturnEmptyPlan()
    {
        ReprioritisationSuggestion suggestion = ReprioritisationSuggestion.Build(
            DayOfWeek.Friday, 6, new List<(TaskId, int)>(), new DateOnly(2026, 4, 17));
        suggestion.Suggestions.ShouldBeEmpty();
        suggestion.ScheduledUnits.ShouldBe(0);
    }
}
