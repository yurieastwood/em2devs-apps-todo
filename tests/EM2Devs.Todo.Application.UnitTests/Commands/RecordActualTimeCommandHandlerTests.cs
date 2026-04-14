using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Validators;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using FluentValidation.Results;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class RecordActualTimeCommandHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly RecordActualTimeCommandHandler _handler;

    public RecordActualTimeCommandHandlerTests()
    {
        _handler = new RecordActualTimeCommandHandler(_repository);
    }

    private static TodoTask CreateDoneTaskWithEstimate(int estimateMinutes = 30)
    {
        TodoTask task = TodoTask.Create(new TaskTitle("Task"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(estimateMinutes));
        task.MoveToInProgress();
        task.MarkAsDone();
        return task;
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_RecordActualTime_When_TaskIsDoneAndHasEstimate()
    {
        // Given
        TodoTask task = CreateDoneTaskWithEstimate(30);
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        RecordActualTimeCommand command = new(task.Id.Value, 45);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        TodoTask updated = result.Match(t => t, _ => null!);
        updated.ActualTimeRecord.ShouldNotBeNull();
        updated.ActualTimeRecord!.Actual.Minutes.ShouldBe(45);
        updated.ActualTimeRecord.Estimated.Minutes.ShouldBe(30);
        await _repository.Received(1).SaveAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_TaskDoesNotExist()
    {
        // Given
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        RecordActualTimeCommand command = new(Guid.NewGuid(), 30);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_TaskIsNotDone()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Task"));
        task.UpdateEstimatedTime(TimeEstimate.FromMinutes(30));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        RecordActualTimeCommand command = new(task.Id.Value, 45);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_TaskHasNoEstimate()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Task"));
        task.MoveToInProgress();
        task.MarkAsDone();
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        RecordActualTimeCommand command = new(task.Id.Value, 45);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_ActualMinutesIsZero()
    {
        // Given
        TodoTask task = CreateDoneTaskWithEstimate();
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        RecordActualTimeCommand command = new(task.Id.Value, 0);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
    }

    [Theory]
    [Trait("Category", "Application")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1441)]
    public void Validator_Should_Reject_OutOfRangeMinutes(int minutes)
    {
        RecordActualTimeCommandValidator validator = new();
        ValidationResult result = validator.Validate(new RecordActualTimeCommand(Guid.NewGuid(), minutes));
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(RecordActualTimeCommand.ActualMinutes));
    }

    [Theory]
    [Trait("Category", "Application")]
    [InlineData(1)]
    [InlineData(45)]
    [InlineData(1440)]
    public void Validator_Should_Accept_InRangeMinutes(int minutes)
    {
        RecordActualTimeCommandValidator validator = new();
        ValidationResult result = validator.Validate(new RecordActualTimeCommand(Guid.NewGuid(), minutes));
        result.IsValid.ShouldBeTrue();
    }
}
