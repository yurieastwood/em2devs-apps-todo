using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Queries;

public sealed class GetTaskQueryHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly GetTaskQueryHandler _handler;

    public GetTaskQueryHandlerTests()
    {
        _handler = new GetTaskQueryHandler(_repository);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnSuccess_When_TaskExists()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Test task"));
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns(task);

        GetTaskQuery query = new(task.Id.Value);

        // When
        Result<TodoTask> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Title.Value, _ => "").ShouldBe("Test task");
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnNotFoundError_When_TaskDoesNotExist()
    {
        // Given
        _repository.GetByIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>())
            .Returns((TodoTask?)null);

        GetTaskQuery query = new(Guid.NewGuid());

        // When
        Result<TodoTask> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<NotFoundError>();
    }
}
