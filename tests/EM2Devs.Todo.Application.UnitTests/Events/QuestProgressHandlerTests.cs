using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Ports;
using TaskStatus = EM2Devs.Todo.Domain.TaskStatus;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

public sealed class QuestProgressHandlerTests
{
    private readonly IQuestRepository _questRepo = Substitute.For<IQuestRepository>();
    private readonly ITaskRepository _taskRepo = Substitute.For<ITaskRepository>();
    private readonly QuestProgressHandler _handler;

    public QuestProgressHandlerTests()
    {
        _handler = new QuestProgressHandler(_questRepo, _taskRepo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReplaceTaskAndSaveQuest_When_TaskBelongsToQuest()
    {
        // Given — quest has a stale task snapshot (status = Todo)
        TodoTask staleTask = TodoTask.Create(new TaskTitle("Test task"));
        Quest quest = Quest.Create(new QuestTitle("Test quest"), "desc");
        quest.AddTask(staleTask);

        quest.Tasks[0].Status.ShouldBe(TaskStatus.Todo);

        // Simulate the task repo returning the same task (now with updated status)
        staleTask.MoveToInProgress();

        _taskRepo.GetByIdAsync(staleTask.Id, Arg.Any<CancellationToken>())
            .Returns(staleTask);

        _questRepo.GetByTaskIdAsync(staleTask.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskStatusChangedEvent evt = new(staleTask.Id, TaskStatus.InProgress);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — quest's task snapshot is replaced with fresh data
        await _questRepo.Received(1).SaveAsync(quest, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotSaveQuest_When_TaskNotInAnyQuest()
    {
        // Given
        TaskId orphanTaskId = new(Guid.NewGuid());
        TodoTask freshTask = TodoTask.Create(new TaskTitle("Orphan"));

        _taskRepo.GetByIdAsync(orphanTaskId, Arg.Any<CancellationToken>())
            .Returns(freshTask);

        _questRepo.GetByTaskIdAsync(orphanTaskId, Arg.Any<CancellationToken>())
            .Returns(new List<Quest>().AsReadOnly());

        TaskStatusChangedEvent evt = new(orphanTaskId, TaskStatus.Done);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _questRepo.DidNotReceive().SaveAsync(Arg.Any<Quest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_DoNothing_When_TaskNotFound()
    {
        // Given
        TaskId deletedTaskId = new(Guid.NewGuid());

        _taskRepo.GetByIdAsync(deletedTaskId, Arg.Any<CancellationToken>())
            .Returns((TodoTask?)null);

        TaskStatusChangedEvent evt = new(deletedTaskId, TaskStatus.Done);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — should not even query quests
        await _questRepo.DidNotReceive().GetByTaskIdAsync(Arg.Any<TaskId>(), Arg.Any<CancellationToken>());
        await _questRepo.DidNotReceive().SaveAsync(Arg.Any<Quest>(), Arg.Any<CancellationToken>());
    }
}
