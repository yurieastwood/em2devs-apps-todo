using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class ReopenTaskCommandHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly ReopenTaskCommandHandler _handler;

    public ReopenTaskCommandHandlerTests()
    {
        _handler = new ReopenTaskCommandHandler(_repository);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReopenTask_When_TaskIsDone()
    {
        // Given
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("Completed task"));
        task.MoveToInProgress();
        task.MarkAsDone();
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        ReopenTaskCommand command = new(task.Id.Value);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Status, _ => default).ShouldBe(Domain.TaskStatus.Todo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_TaskIsNotDone()
    {
        // Given
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("Not done"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        ReopenTaskCommand command = new(task.Id.Value);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_TaskDoesNotExist()
    {
        // Given
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        ReopenTaskCommand command = new(Guid.NewGuid());

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }
}
