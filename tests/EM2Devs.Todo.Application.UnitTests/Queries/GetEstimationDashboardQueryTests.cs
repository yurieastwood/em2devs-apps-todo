using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Queries;

[Trait("Category", "Application")]
public sealed class GetEstimationDashboardQueryTests
{
    private static readonly Guid _testUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly GetEstimationDashboardQueryHandler _handler;

    public GetEstimationDashboardQueryTests()
    {
        _handler = new GetEstimationDashboardQueryHandler(_taskRepository);
    }

    [Fact]
    public async Task Should_ReturnImprovementMessage_When_RecentAccuracyBetterThanEarly()
    {
        var tasks = new List<TodoTask>();
        for (int i = 0; i < 4; i++)
        {
            tasks.Add(CompletedWithVariance(60, 120, DateTimeOffset.UtcNow.AddDays(-30 + i)));
        }

        for (int i = 0; i < 4; i++)
        {
            tasks.Add(CompletedWithVariance(60, 65, DateTimeOffset.UtcNow.AddDays(-5 + i)));
        }

        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tasks);

        Result<EstimationDashboardReadModel> result =
            await _handler.Handle(new GetEstimationDashboardQuery(), default);

        EstimationDashboardReadModel dashboard = result.Match(d => d, _ => throw new Xunit.Sdk.XunitException("expected success"));
        dashboard.ImprovementMessage.ShouldNotBeNull();
        dashboard.ImprovementMessage.ShouldContain("improved");
    }

    [Fact]
    public async Task Should_ReturnNullImprovement_When_NotEnoughData()
    {
        var tasks = new List<TodoTask>
        {
            CompletedWithVariance(60, 90, DateTimeOffset.UtcNow.AddDays(-1)),
            CompletedWithVariance(60, 80, DateTimeOffset.UtcNow),
        };

        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(tasks);

        Result<EstimationDashboardReadModel> result =
            await _handler.Handle(new GetEstimationDashboardQuery(), default);

        EstimationDashboardReadModel dashboard = result.Match(d => d, _ => throw new Xunit.Sdk.XunitException("expected success"));
        dashboard.ImprovementMessage.ShouldBeNull();
    }

    private static TodoTask CompletedWithVariance(int estimated, int actual, DateTimeOffset completedAt)
    {
        TodoTask task = TodoTask.Create(_testUserId, new TaskTitle("estimation test"), createdAt: completedAt.AddHours(-1));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(estimated));
        task.MoveToInProgress();
        task.MarkAsDone();
        task.RecordActualTime(TimeEstimate.FromMinutes(actual));
        return task;
    }
}
