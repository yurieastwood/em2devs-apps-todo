using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class DeleteTaskCommandHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly DeleteTaskCommandHandler _handler;

    public DeleteTaskCommandHandlerTests()
    {
        _handler = new DeleteTaskCommandHandler(_repository);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnSuccess_When_TaskExists()
    {
        // Given
        _repository.DeleteAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(true);

        DeleteTaskCommand command = new(Guid.NewGuid());

        // When
        Result<bool> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_TaskDoesNotExist()
    {
        // Given
        _repository.DeleteAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        DeleteTaskCommand command = new(Guid.NewGuid());

        // When
        Result<bool> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }
}
