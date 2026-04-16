using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Events;

/// <summary>
/// Tests for XP attribution to parent quest when a task in a quest is completed.
/// Scenario: "XP correctly attributed to parent quest"
/// </summary>
public sealed class XpQuestAttributionHandlerTests
{
    private readonly IPlayerProfileRepository _profileRepo = Substitute.For<IPlayerProfileRepository>();
    private readonly IQuestRepository _questRepo = Substitute.For<IQuestRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly XpAwardHandler _handler;

    public XpQuestAttributionHandlerTests()
    {
        _handler = new XpAwardHandler(_profileRepo, _mediator, _questRepo);
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AttributeXpToQuest_When_TaskBelongsToQuest()
    {
        // Given — a task that belongs to a quest
        TaskId taskId = new(Guid.NewGuid());
        Quest quest = Quest.Create(new QuestTitle("Sprint work"), "Sprint tasks");
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("Fix login bug"));
        quest.AddTask(task);

        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(100, 2, 50, 66, 3, 5));

        _questRepo.GetByTaskIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest }.AsReadOnly());

        TaskCompletedEvent evt = new(
            taskId,
            new TaskTitle("Fix login bug"),
            TaskDifficulty.Normal);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — XP should be attributed to the quest
        quest.TotalXpEarned.Value.ShouldBeGreaterThan(0);
        await _questRepo.Received(1).SaveAsync(quest, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_NotAttributeXpToQuest_When_TaskHasNoQuest()
    {
        // Given — a task with no quest
        TaskId taskId = new(Guid.NewGuid());

        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(100, 2, 50, 66, 3, 5));

        _questRepo.GetByTaskIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(new List<Quest>().AsReadOnly());

        TaskCompletedEvent evt = new(
            taskId,
            new TaskTitle("Standalone task"),
            TaskDifficulty.Normal);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — no quest save needed
        await _questRepo.DidNotReceive().SaveAsync(Arg.Any<Quest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_AttributeXpToMultipleQuests_When_TaskInMultipleQuests()
    {
        // Given — a task that belongs to multiple quests
        TaskId taskId = new(Guid.NewGuid());
        Quest quest1 = Quest.Create(new QuestTitle("Quest 1"), "First");
        Quest quest2 = Quest.Create(new QuestTitle("Quest 2"), "Second");
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("Shared task"));
        quest1.AddTask(task);
        quest2.AddTask(TodoTask.Create(TestUserId, new TaskTitle("Shared task"))); // Different instance but for testing

        _profileRepo.GetProfileAsync(Arg.Any<CancellationToken>())
            .Returns(new PlayerProfileReadModel(100, 2, 50, 66, 0, 0));

        _questRepo.GetByTaskIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns(new List<Quest> { quest1, quest2 }.AsReadOnly());

        TaskCompletedEvent evt = new(
            taskId,
            new TaskTitle("Shared task"),
            TaskDifficulty.Normal);

        // When
        await _handler.Handle(evt, CancellationToken.None);

        // Then — XP attributed to both quests
        quest1.TotalXpEarned.Value.ShouldBeGreaterThan(0);
        quest2.TotalXpEarned.Value.ShouldBeGreaterThan(0);
        await _questRepo.Received(2).SaveAsync(Arg.Any<Quest>(), Arg.Any<CancellationToken>());
    }
}
