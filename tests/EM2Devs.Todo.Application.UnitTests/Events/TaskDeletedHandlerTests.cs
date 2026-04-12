using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

/// <summary>
/// Tests for TaskDeletedHandler.
/// Scenario: "Delete a task that belongs to a quest" — quest progress should recalculate.
/// </summary>
public sealed class TaskDeletedHandlerTests
{
    private readonly IQuestRepository _questRepo = Substitute.For<IQuestRepository>();
    private readonly TaskDeletedHandler _handler;

    public TaskDeletedHandlerTests()
    {
        _handler = new TaskDeletedHandler(_questRepo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_RemoveTaskFromQuestAndSave_When_TaskBelongsToQuest()
    {
        // Given — quest with 4 tasks, we delete one
        Quest quest = Quest.Create(new QuestTitle("Launch campaign"), "Test");
        TodoTask task1 = TodoTask.Create(new TaskTitle("Design flyer"));
        TodoTask task2 = TodoTask.Create(new TaskTitle("Write copy"));
        TodoTask task3 = TodoTask.Create(new TaskTitle("Review"));
        TodoTask task4 = TodoTask.Create(new TaskTitle("Publish"));
        quest.AddTask(task1);
        quest.AddTask(task2);
        quest.AddTask(task3);
        quest.AddTask(task4);

        _questRepo.GetByTaskIdAsync(task1.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskDeletedEvent evt = new(task1.Id);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — quest should now have 3 tasks
        quest.Tasks.Count.ShouldBe(3);
        await _questRepo.Received(1).SaveAsync(quest, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_RecalculateQuestProgress_When_TaskDeletedFromQuest()
    {
        // Given — quest with 4 tasks, 2 completed. Progress = 50%.
        // Delete one incomplete task → 2/3 completed = 66%.
        Quest quest = Quest.Create(new QuestTitle("Launch campaign"), "Test");
        TodoTask task1 = TodoTask.Create(new TaskTitle("Task 1"));
        TodoTask task2 = TodoTask.Create(new TaskTitle("Task 2"));
        TodoTask task3 = TodoTask.Create(new TaskTitle("Task 3"));
        TodoTask task4 = TodoTask.Create(new TaskTitle("Task 4"));
        quest.AddTask(task1);
        quest.AddTask(task2);
        quest.AddTask(task3);
        quest.AddTask(task4);

        task1.MoveToInProgress();
        task1.MarkAsDone();
        quest.ReplaceTask(task1);
        task2.MoveToInProgress();
        task2.MarkAsDone();
        quest.ReplaceTask(task2);

        quest.Progress.ShouldBe(50);

        _questRepo.GetByTaskIdAsync(task3.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskDeletedEvent evt = new(task3.Id);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — progress recalculated: 2/3 = 66%
        quest.Progress.ShouldBe(66);
        quest.Tasks.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotSaveQuest_When_TaskNotInAnyQuest()
    {
        // Given
        TaskId orphanTaskId = new(Guid.NewGuid());

        _questRepo.GetByTaskIdAsync(orphanTaskId, Arg.Any<CancellationToken>())
            .Returns(new List<Quest>().AsReadOnly());

        TaskDeletedEvent evt = new(orphanTaskId);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _questRepo.DidNotReceive().SaveAsync(Arg.Any<Quest>(), Arg.Any<CancellationToken>());
    }
}
