using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for TaskTimer: optional time tracking during task execution.
/// </summary>
public sealed class TaskTimerTests
{
    private static readonly DateTimeOffset _start = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportRunning_When_NotYetStopped()
    {
        TaskTimer timer = TaskTimer.Start(_start);
        timer.IsRunning.ShouldBeTrue();
        timer.Elapsed.ShouldBe(TimeSpan.Zero);
        timer.StoppedAt.ShouldBeNull();
        timer.StartedAt.ShouldBe(_start);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReportElapsedTime_When_Stopped()
    {
        TaskTimer timer = TaskTimer.Start(_start).Stop(_start.AddMinutes(45));
        timer.IsRunning.ShouldBeFalse();
        timer.Elapsed.ShouldBe(TimeSpan.FromMinutes(45));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ConvertToTimeEstimate_When_Stopped()
    {
        TaskTimer timer = TaskTimer.Start(_start).Stop(_start.AddMinutes(45));
        TimeEstimate estimate = timer.ToTimeEstimate();
        estimate.Minutes.ShouldBe(45);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RoundUpToWholeMinutes_When_ConvertingToEstimate()
    {
        TaskTimer timer = TaskTimer.Start(_start).Stop(_start.AddSeconds(45 * 60 + 1));
        timer.ToTimeEstimate().Minutes.ShouldBe(46);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnMinimumOneMinute_When_StoppedImmediately()
    {
        TaskTimer timer = TaskTimer.Start(_start).Stop(_start);
        timer.ToTimeEstimate().Minutes.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_StoppingAlreadyStoppedTimer()
    {
        TaskTimer timer = TaskTimer.Start(_start).Stop(_start.AddMinutes(30));
        DomainException ex = Should.Throw<DomainException>(() => timer.Stop(_start.AddMinutes(45)));
        ex.Message.ShouldContain("already been stopped");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_StopBeforeStart()
    {
        TaskTimer timer = TaskTimer.Start(_start);
        DomainException ex = Should.Throw<DomainException>(() => timer.Stop(_start.AddMinutes(-1)));
        ex.Message.ShouldContain("cannot precede start");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Throw_When_ConvertingRunningTimer()
    {
        TaskTimer timer = TaskTimer.Start(_start);
        DomainException ex = Should.Throw<DomainException>(() => timer.ToTimeEstimate());
        ex.Message.ShouldContain("must be stopped");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnElapsedMinutes_When_AboveMinimum()
    {
        // Kills `minutes < 1` -> `minutes <= 1` mutation: value 2 should not be bumped to 1.
        TaskTimer timer = TaskTimer.Start(_start).Stop(_start.AddMinutes(2));
        timer.ToTimeEstimate().Minutes.ShouldBe(2);
    }
}
