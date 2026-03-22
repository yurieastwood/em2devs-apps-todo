using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.Queries;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Queries;

public sealed class ListTasksQueryHandlerTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly ListTasksQueryHandler _handler;

    public ListTasksQueryHandlerTests()
    {
        _handler = new ListTasksQueryHandler(_repository);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnSuccess_When_TasksExist()
    {
        // Given
        List<TodoTask> tasks = [TodoTask.Create(new TaskTitle("Task 1")), TodoTask.Create(new TaskTitle("Task 2"))];
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(tasks.AsReadOnly());

        ListTasksQuery query = new(null);

        // When
        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Count, _ => -1).ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnSuccess_When_NoTasksExist()
    {
        // Given
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<TodoTask>().AsReadOnly());

        ListTasksQuery query = new(null);

        // When
        Result<IReadOnlyList<TodoTask>> result = await _handler.Handle(query, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        result.Match(t => t.Count, _ => -1).ShouldBe(0);
    }
}
