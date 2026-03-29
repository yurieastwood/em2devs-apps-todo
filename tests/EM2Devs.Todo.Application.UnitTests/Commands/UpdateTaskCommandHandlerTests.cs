using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class UpdateTaskCommandHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly UpdateTaskCommandHandler _handler;

    public UpdateTaskCommandHandlerTests()
    {
        _handler = new UpdateTaskCommandHandler(_repository);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_UpdateTitle_When_ValidTitleProvided()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Old title"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        UpdateTaskCommand command = new(task.Id.Value, Title: "New title");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Title.Value, _ => "").ShouldBe("New title");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_UpdateDescription_When_DescriptionProvided()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Test"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        UpdateTaskCommand command = new(task.Id.Value, Description: "New description");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Description, _ => null).ShouldBe("New description");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_TaskDoesNotExist()
    {
        // Given
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        UpdateTaskCommand command = new(Guid.NewGuid(), Title: "Doesn't matter");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_TitleIsEmpty()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Test"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        UpdateTaskCommand command = new(task.Id.Value, Title: "");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_DifficultyIsInvalid()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Test"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>()).Returns(task);

        UpdateTaskCommand command = new(task.Id.Value, Difficulty: "Legendary");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
    }
}
