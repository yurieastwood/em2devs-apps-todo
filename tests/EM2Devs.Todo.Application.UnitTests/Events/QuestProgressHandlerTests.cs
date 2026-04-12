using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Mediator;
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
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly QuestProgressHandler _handler;

    public QuestProgressHandlerTests()
    {
        _handler = new QuestProgressHandler(_questRepo, _taskRepo, _mediator);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReplaceTaskAndSaveQuest_When_TaskBelongsToQuest()
    {
        // Given — quest has a stale task snapshot (status = Todo)
        TodoTask staleTask = TodoTask.Create(new TaskTitle("Test task"));
        Quest quest = Quest.Create(new QuestTitle("Test quest"), "desc");
        quest.AddTask(staleTask);
        // Add another task so quest doesn't auto-complete
        quest.AddTask(TodoTask.Create(new TaskTitle("Other task")));

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

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AutoCompleteQuestAndPublishEvent_When_FinalTaskCompleted()
    {
        // Given — quest with 5 tasks, 4 completed, 1 remaining
        Quest quest = Quest.Create(new QuestTitle("Prepare presentation"), "desc");
        for (int i = 0; i < 4; i++)
        {
            TodoTask completedTask = TodoTask.Create(new TaskTitle($"Completed task {i + 1}"));
            completedTask.MoveToInProgress();
            completedTask.MarkAsDone();
            quest.AddTask(completedTask);
        }

        TodoTask finalTask = TodoTask.Create(new TaskTitle("Do final rehearsal"));
        quest.AddTask(finalTask);
        quest.Progress.ShouldBe(80);

        // Simulate the final task being completed
        finalTask.MoveToInProgress();
        finalTask.MarkAsDone();

        _taskRepo.GetByIdAsync(finalTask.Id, Arg.Any<CancellationToken>())
            .Returns(finalTask);

        _questRepo.GetByTaskIdAsync(finalTask.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskStatusChangedEvent evt = new(finalTask.Id, TaskStatus.Done);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — quest should be auto-completed
        quest.IsCompleted.ShouldBeTrue();
        quest.Progress.ShouldBe(100);

        // And saved twice: once for task replace, once for complete
        await _questRepo.Received(2).SaveAsync(quest, Arg.Any<CancellationToken>());

        // And QuestCompletedEvent published
        await _mediator.Received(1).Publish(
            Arg.Is<QuestCompletedEvent>(e => e.QuestId == quest.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotAutoComplete_When_QuestAlreadyCompleted()
    {
        // Given — quest already completed, task status changes for some reason
        Quest quest = Quest.Create(new QuestTitle("Already done"), "desc");
        TodoTask task = TodoTask.Create(new TaskTitle("Task"));
        task.MoveToInProgress();
        task.MarkAsDone();
        quest.AddTask(task);
        quest.Complete();

        _taskRepo.GetByIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(task);

        _questRepo.GetByTaskIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskStatusChangedEvent evt = new(task.Id, TaskStatus.Done);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — should save for task replace but not publish completion event
        await _questRepo.Received(1).SaveAsync(quest, Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Publish(
            Arg.Any<QuestCompletedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotAutoComplete_When_NotAllTasksDone()
    {
        // Given — quest with 3 tasks, 1 completed + 1 being completed = 2/3
        Quest quest = Quest.Create(new QuestTitle("Partial"), "desc");
        TodoTask doneTask = TodoTask.Create(new TaskTitle("Done"));
        doneTask.MoveToInProgress();
        doneTask.MarkAsDone();
        quest.AddTask(doneTask);

        TodoTask currentTask = TodoTask.Create(new TaskTitle("Current"));
        quest.AddTask(currentTask);

        TodoTask futureTask = TodoTask.Create(new TaskTitle("Future"));
        quest.AddTask(futureTask);

        // Simulate current task being completed
        currentTask.MoveToInProgress();
        currentTask.MarkAsDone();

        _taskRepo.GetByIdAsync(currentTask.Id, Arg.Any<CancellationToken>())
            .Returns(currentTask);

        _questRepo.GetByTaskIdAsync(currentTask.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskStatusChangedEvent evt = new(currentTask.Id, TaskStatus.Done);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — quest not completed, progress = 66%
        quest.IsCompleted.ShouldBeFalse();
        quest.Progress.ShouldBe(66);
        await _mediator.DidNotReceive().Publish(
            Arg.Any<QuestCompletedEvent>(),
            Arg.Any<CancellationToken>());
    }
}
