using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class TodoTaskActualTimeTests
{
    private static TodoTask NewCompleted(int estimatedMinutes = 120)
    {
        var task = TodoTask.Create(new TaskTitle("Write Q2 report"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(estimatedMinutes));
        task.MoveToInProgress();
        task.MarkAsDone();
        return task;
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecordVariance_When_ActualTimeLoggedAfterCompletion()
    {
        // 2h estimated, 2h45m actual -> +37.5%
        var task = NewCompleted(estimatedMinutes: 120);
        var record = task.RecordActualTime(TimeEstimate.FromMinutes(165));

        record.VariancePercent.ShouldBe(37.5);
        record.Estimated.Minutes.ShouldBe(120);
        record.Actual.Minutes.ShouldBe(165);
        task.ActualTimeRecord.ShouldBe(record);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NotYetCompleted()
    {
        var task = TodoTask.Create(new TaskTitle("x"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(60));

        Should.Throw<DomainException>(() =>
            task.RecordActualTime(TimeEstimate.FromMinutes(60)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NoEstimate()
    {
        var task = TodoTask.Create(new TaskTitle("x"));
        task.MoveToInProgress();
        task.MarkAsDone();

        Should.Throw<DomainException>(() =>
            task.RecordActualTime(TimeEstimate.FromMinutes(60)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ActualNull()
    {
        var task = NewCompleted();
        Should.Throw<ArgumentNullException>(() => task.RecordActualTime(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_Before_StatusCheck_When_ActualNull()
    {
        // Task is NOT Done — if null check runs first, we get ArgumentNullException.
        // If null check is removed, we'd get DomainException for status instead.
        var task = TodoTask.Create(new TaskTitle("Not done task"));
        var ex = Should.Throw<ArgumentNullException>(() => task.RecordActualTime(null!));
        ex.ParamName.ShouldBe("actual");
    }
}
