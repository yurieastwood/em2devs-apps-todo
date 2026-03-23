using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for Epic entity.
/// Tests encode behaviors from quest-hierarchy.feature (ADR-0003).
/// </summary>
public sealed class EpicTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateEpic_When_ValidDetailsProvided()
    {
        // Given
        EpicTitle title = new("Launch MVP");
        string description = "Ship the first public version of the app";
        DateOnly targetDate = new(2026, 9, 1);

        // When
        Epic epic = Epic.Create(title, description, targetDate);

        // Then
        epic.Id.Value.ShouldNotBe(Guid.Empty);
        epic.Title.ShouldBe(title);
        epic.Description.ShouldBe(description);
        epic.TargetDate.ShouldBe(targetDate);
        epic.Progress.ShouldBe(0m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateEpicWithoutTargetDate_When_TargetDateOmitted()
    {
        // Given
        EpicTitle title = new("Open-ended epic");

        // When
        Epic epic = Epic.Create(title, "No deadline");

        // Then
        epic.TargetDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicTitleIsEmpty()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new EpicTitle(""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicTitleExceedsMaxLength()
    {
        // Given
        string longTitle = new('x', 201);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => new EpicTitle(longTitle));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptEpicTitle_When_TitleIsExactlyMaxLength()
    {
        // Given
        string maxTitle = new('x', 200);

        // When
        EpicTitle title = new(maxTitle);

        // Then
        title.Value.ShouldBe(maxTitle);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicTitleIsNull()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new EpicTitle(null!));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicTitleIsWhitespaceOnly()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new EpicTitle("   "));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicTitleIsControlCharactersOnly()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new EpicTitle("\x01\x02\x03"));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainQuests_When_QuestsAreAdded()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest1 = Quest.Create(new QuestTitle("Build authentication"), "Auth module");
        Quest quest2 = Quest.Create(new QuestTitle("Design UI"), "UI work");

        // When
        epic.AddQuest(quest1);
        epic.AddQuest(quest2);

        // Then
        epic.Quests.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroProgress_When_NoQuestsExist()
    {
        // Given
        Epic epic = CreateEpic();

        // Then
        epic.Progress.ShouldBe(0m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroProgress_When_NoQuestsHaveProgress()
    {
        // Given
        Epic epic = CreateEpicWithQuests(4);

        // Then
        epic.Progress.ShouldBe(0m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateEqualWeightedProgress_When_QuestsHaveMixedCompletion()
    {
        // Given — scenario from feature file:
        // Quest 1: 100% complete (2/2 tasks done)
        // Quest 2: 50% complete (1/2 tasks done)
        // Quest 3: 0% complete
        // Quest 4: 0% complete
        // Expected: (100 + 50 + 0 + 0) / 4 = 37.5%
        Epic epic = CreateEpic();

        Quest quest1 = CreateQuestWithCompletedTasks(2, 2);
        Quest quest2 = CreateQuestWithCompletedTasks(2, 1);
        Quest quest3 = CreateQuestWithCompletedTasks(2, 0);
        Quest quest4 = CreateQuestWithCompletedTasks(2, 0);

        epic.AddQuest(quest1);
        epic.AddQuest(quest2);
        epic.AddQuest(quest3);
        epic.AddQuest(quest4);

        // Then
        epic.Progress.ShouldBe(37.5m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveFullProgress_When_AllQuestsAreComplete()
    {
        // Given
        Epic epic = CreateEpic();

        Quest quest1 = CreateQuestWithCompletedTasks(3, 3);
        Quest quest2 = CreateQuestWithCompletedTasks(2, 2);

        epic.AddQuest(quest1);
        epic.AddQuest(quest2);

        // Then
        epic.Progress.ShouldBe(100m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DuplicateQuestIsAdded()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest = Quest.Create(new QuestTitle("Build auth"), "Auth");
        epic.AddQuest(quest);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => epic.AddQuest(quest));
        ex.Message.ShouldContain("already");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DuplicateQuestIsAddedAmongMultiple()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest1 = Quest.Create(new QuestTitle("First quest"), "First");
        Quest quest2 = Quest.Create(new QuestTitle("Second quest"), "Second");
        epic.AddQuest(quest1);
        epic.AddQuest(quest2);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => epic.AddQuest(quest1));
        ex.Message.ShouldContain("already");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullQuest()
    {
        // Given
        Epic epic = CreateEpic();

        // When / Then
        Should.Throw<ArgumentNullException>(() => epic.AddQuest(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_WeightQuestsEqually_RegardlessOfTaskCount()
    {
        // Given — quest with 10 tasks (all done) and quest with 1 task (none done)
        // Each contributes 50% to epic progress: (100 + 0) / 2 = 50%
        Epic epic = CreateEpic();

        Quest bigQuest = CreateQuestWithCompletedTasks(10, 10);
        Quest smallQuest = CreateQuestWithCompletedTasks(1, 0);

        epic.AddQuest(bigQuest);
        epic.AddQuest(smallQuest);

        // Then
        epic.Progress.ShouldBe(50m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveQuest_When_QuestIsAssigned()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest = Quest.Create(new QuestTitle("Removable"), "Test");
        epic.AddQuest(quest);

        // When
        epic.RemoveQuest(quest.Id);

        // Then
        epic.Quests.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingUnassignedQuest()
    {
        // Given
        Epic epic = CreateEpic();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => epic.RemoveQuest(new QuestId(Guid.NewGuid())));
        ex.Message.ShouldContain("not assigned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RemovingNullQuestId()
    {
        // Given
        Epic epic = CreateEpic();

        // When / Then
        Should.Throw<ArgumentNullException>(() => epic.RemoveQuest(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecalculateProgress_When_QuestRemoved()
    {
        // Given — 2 quests, one 100% and one 0%. Remove 0% → 100%
        Epic epic = CreateEpic();
        Quest doneQuest = CreateQuestWithCompletedTasks(2, 2);
        Quest emptyQuest = Quest.Create(new QuestTitle("Empty"), "No tasks");
        epic.AddQuest(doneQuest);
        epic.AddQuest(emptyQuest);
        epic.Progress.ShouldBe(50m);

        // When
        epic.RemoveQuest(emptyQuest.Id);

        // Then
        epic.Progress.ShouldBe(100m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Complete_When_AllQuestsAreDone()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest = CreateQuestWithCompletedTasks(2, 2);
        epic.AddQuest(quest);

        // When / Then — should not throw
        Should.NotThrow(() => epic.Complete());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetIsCompleted_When_EpicCompleted()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest = CreateQuestWithCompletedTasks(1, 1);
        epic.AddQuest(quest);

        // When
        epic.Complete();

        // Then
        epic.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingAlreadyCompletedEpic()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest = CreateQuestWithCompletedTasks(1, 1);
        epic.AddQuest(quest);
        epic.Complete();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => epic.Complete());
        ex.Message.ShouldContain("already completed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingWithNoQuests()
    {
        // Given
        Epic epic = CreateEpic();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => epic.Complete());
        ex.Message.ShouldContain("no quests");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingWithIncompleteQuests()
    {
        // Given
        Epic epic = CreateEpic();
        Quest quest = CreateQuestWithCompletedTasks(3, 1);
        epic.AddQuest(quest);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => epic.Complete());
        ex.Message.ShouldContain("not all quests are done");
    }

    private static Epic CreateEpic()
    {
        return Epic.Create(
            new EpicTitle("Test epic"),
            "A test epic",
            new DateOnly(2026, 12, 31));
    }

    private static Epic CreateEpicWithQuests(int count)
    {
        Epic epic = CreateEpic();
        for (int i = 0; i < count; i++)
        {
            epic.AddQuest(Quest.Create(new QuestTitle($"Quest {i + 1}"), $"Description {i + 1}"));
        }

        return epic;
    }

    private static Quest CreateQuestWithCompletedTasks(int totalTasks, int completedTasks)
    {
        Quest quest = Quest.Create(new QuestTitle($"Quest-{Guid.NewGuid():N}"), "Test quest");
        for (int i = 0; i < totalTasks; i++)
        {
            TodoTask task = TodoTask.Create(new TaskTitle($"Task {i + 1}"));
            quest.AddTask(task);

            if (i < completedTasks)
            {
                task.MoveToInProgress();
                task.MarkAsDone();
            }
        }

        return quest;
    }
}
