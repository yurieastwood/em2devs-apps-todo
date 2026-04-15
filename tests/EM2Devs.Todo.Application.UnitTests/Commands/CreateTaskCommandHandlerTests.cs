using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class CreateTaskCommandHandlerTests
{
    private static readonly string[] _mixedCaseTagsInput = ["work", "Milestone"];
    private static readonly string[] _normalisedTagsExpected = ["work", "milestone"];
    private static readonly string[] _whitespaceTagInput = ["  "];

    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _currentUser.UserId.Returns(TestUserId);
        _handler = new CreateTaskCommandHandler(_repository, _currentUser);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnSuccess_When_ValidTitleProvided()
    {
        // Given
        CreateTaskCommand command = new("Buy groceries");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Title.Value, _ => "").ShouldBe("Buy groceries");
        await _repository.Received(1).SaveAsync(Arg.Any<TodoTask>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_TitleIsEmpty()
    {
        // Given
        CreateTaskCommand command = new("");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
        await _repository.DidNotReceive().SaveAsync(Arg.Any<TodoTask>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_SetScheduledDate_When_Provided()
    {
        // Given
        DateOnly scheduled = new(2026, 4, 15);
        CreateTaskCommand command = new("Write report", ScheduledDate: scheduled);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        TodoTask task = result.Match(t => t, _ => null!);
        task.ScheduledDate.ShouldBe(scheduled);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AttachTags_When_TagsProvided()
    {
        // Given
        CreateTaskCommand command = new("Ship demo", Tags: _mixedCaseTagsInput);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        TodoTask task = result.Match(t => t, _ => null!);
        task.Tags.Select(t => t.Value).ShouldBe(_normalisedTagsExpected);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_TagIsInvalid()
    {
        // Given — whitespace tag fails Tag.From validation
        CreateTaskCommand command = new("Task", Tags: _whitespaceTagInput);

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
        await _repository.DidNotReceive().SaveAsync(Arg.Any<TodoTask>(), Arg.Any<CancellationToken>());
    }
}
