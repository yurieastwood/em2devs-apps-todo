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
public sealed class GetEstimationBiasQueryTests
{
    private readonly ITaskRepository _taskRepository = Substitute.For<ITaskRepository>();
    private readonly GetEstimationBiasQueryHandler _handler;

    public GetEstimationBiasQueryTests()
    {
        _handler = new GetEstimationBiasQueryHandler(_taskRepository);
    }

    private static TodoTask TaskWithActual(int estimatedMinutes, int actualMinutes)
    {
        TodoTask task = TodoTask.Create(TestData.TestUserId, new TaskTitle("t"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(estimatedMinutes));
        task.MoveToInProgress();
        task.MarkAsDone();
        task.RecordActualTime(TimeEstimate.FromMinutes(actualMinutes));
        return task;
    }

    [Fact]
    public async Task Should_ReturnNotEnoughDataState_When_NoTasksHaveActualTimes()
    {
        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<TodoTask>());

        Result<EstimationCalibrationReadModel> result = await _handler.Handle(new GetEstimationBiasQuery(), default);

        result.IsSuccess.ShouldBeTrue();
        EstimationCalibrationReadModel calibration = result.Match(r => r, _ => throw new Xunit.Sdk.XunitException("expected success"));
        calibration.CalibrationState.ShouldBe("NotEnoughData");
        calibration.BiasFactor.ShouldBe(1.0);
        calibration.SampleSize.ShouldBe(0);
    }

    [Fact]
    public async Task Should_ReturnCalibratedState_When_EnoughSamplesExist()
    {
        _taskRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([
            TaskWithActual(30, 45),
            TaskWithActual(60, 90),
            TaskWithActual(20, 30),
        ]);

        Result<EstimationCalibrationReadModel> result = await _handler.Handle(new GetEstimationBiasQuery(), default);

        EstimationCalibrationReadModel calibration = result.Match(r => r, _ => throw new Xunit.Sdk.XunitException("expected success"));
        calibration.CalibrationState.ShouldBe("Calibrated");
        calibration.BiasFactor.ShouldBe(1.5);
        calibration.SampleSize.ShouldBe(3);
    }
}
