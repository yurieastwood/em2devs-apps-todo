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
    private readonly QuestProgressHandler _handler;

    public QuestProgressHandlerTests()
    {
        _handler = new QuestProgressHandler(_questRepo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_SaveQuest_When_TaskBelongsToQuest()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Test task"));
        Quest quest = Quest.Create(new QuestTitle("Test quest"), "desc");
        quest.AddTask(task);

        _questRepo.GetByTaskIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskStatusChangedEvent evt = new(task.Id, TaskStatus.InProgress);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _questRepo.Received(1).SaveAsync(quest, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotSaveQuest_When_TaskNotInAnyQuest()
    {
        // Given
        TaskId orphanTaskId = new(Guid.NewGuid());

        _questRepo.GetByTaskIdAsync(orphanTaskId, Arg.Any<CancellationToken>())
            .Returns(new List<Quest>().AsReadOnly());

        TaskStatusChangedEvent evt = new(orphanTaskId, TaskStatus.Done);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then
        await _questRepo.DidNotReceive().SaveAsync(Arg.Any<Quest>(), Arg.Any<CancellationToken>());
    }
}
