using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class QuickAddTaskCommandHandlerTests
{
    private static readonly string[] _expectedTags = ["groceries"];

    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly IRecurringTaskRepository _recurringRepository = Substitute.For<IRecurringTaskRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 4, 12, 12, 0, 0, TimeSpan.Zero));
    private readonly QuickAddTaskCommandHandler _handler;

    public QuickAddTaskCommandHandlerTests()
    {
        _currentUser.UserId.Returns(TestUserId);
        _handler = new QuickAddTaskCommandHandler(_repository, _recurringRepository, _currentUser, _timeProvider);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_CreateTask_When_InputHasOnlyTitle()
    {
        // Given
        QuickAddTaskCommand command = new("Write blog post");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        TodoTask task = result.Match(t => t, _ => null!);
        task.Title.Value.ShouldBe("Write blog post");
        task.Tags.ShouldBeEmpty();
        task.ScheduledDate.ShouldBeNull();
        await _repository.Received(1).SaveAsync(Arg.Any<TodoTask>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ParseFullSyntax_When_InputHasAllDirectives()
    {
        // Given — today is 2026-04-12, so ^tomorrow -> 2026-04-13
        QuickAddTaskCommand command = new("buy milk #groceries !High ^tomorrow");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsSuccess.ShouldBeTrue();
        TodoTask task = result.Match(t => t, _ => null!);
        task.Title.Value.ShouldBe("buy milk");
        task.Tags.Select(t => t.Value).ShouldBe(_expectedTags);
        task.Priority.ShouldBe(Domain.ValueObjects.TaskPriority.High);
        task.ScheduledDate.ShouldBe(new DateOnly(2026, 4, 13));
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_InputIsEmpty()
    {
        // Given
        QuickAddTaskCommand command = new("");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
        await _repository.DidNotReceive().SaveAsync(Arg.Any<TodoTask>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_OnlyDirectivesProvided()
    {
        // Given — no title tokens, parser throws
        QuickAddTaskCommand command = new("#tag");

        // When
        Result<TodoTask> result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FakeTimeProvider(DateTimeOffset now) => _now = now;
        public override DateTimeOffset GetUtcNow() => _now;
    }
}
