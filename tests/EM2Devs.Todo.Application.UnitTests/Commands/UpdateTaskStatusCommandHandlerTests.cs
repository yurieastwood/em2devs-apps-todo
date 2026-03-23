using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class UpdateTaskStatusCommandHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly UpdateTaskStatusCommandHandler _handler;

    public UpdateTaskStatusCommandHandlerTests()
    {
        _handler = new UpdateTaskStatusCommandHandler(_repository, _mediator);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnSuccess_When_ValidTransitionToInProgress()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Test task"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(task);

        UpdateTaskStatusCommand command = new(task.Id.Value, "InProgress");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Status, _ => default).ShouldBe(Domain.TaskStatus.InProgress);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_TaskDoesNotExist()
    {
        // Given
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns((TodoTask?)null);

        UpdateTaskStatusCommand command = new(Guid.NewGuid(), "InProgress");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_StatusValueIsInvalid()
    {
        // Given
        UpdateTaskStatusCommand command = new(Guid.NewGuid(), "BogusStatus");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_TaskAlreadyInTargetStatus()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Test task"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(task);

        UpdateTaskStatusCommand command = new(task.Id.Value, "Todo");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_PublishEventWithDifficultyAndDeadline_When_TaskCompleted()
    {
        // Given — task with Hard difficulty and a deadline
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddDays(3);
        TodoTask task = TodoTask.Create(new TaskTitle("Hard task"), TaskDifficulty.Hard, deadline);
        task.MoveToInProgress();
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(task);

        UpdateTaskStatusCommand command = new(task.Id.Value, "Done");

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _mediator.Received(1).Publish(
            Arg.Is<Application.Events.TaskCompletedEvent>(e =>
                e.Difficulty == TaskDifficulty.Hard &&
                e.Deadline == deadline &&
                e.CompletedAt != null),
            Arg.Any<CancellationToken>());
    }
}
