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
}
