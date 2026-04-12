using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Gate 4: Scenario-driven tests for Quest entity.
/// Tests encode behaviors from quest-hierarchy.feature (ADR-0003).
/// </summary>
public sealed class QuestTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateQuest_When_ValidDetailsProvided()
    {
        // Given
        QuestTitle title = new("Prepare conference talk");
        string description = "Write and rehearse DDD talk for NDC";
        DateOnly dueDate = new(2026, 6, 1);

        // When
        Quest quest = Quest.Create(title, description, dueDate);

        // Then
        quest.Id.Value.ShouldNotBe(Guid.Empty);
        quest.Title.ShouldBe(title);
        quest.Description.ShouldBe(description);
        quest.DueDate.ShouldBe(dueDate);
        quest.Progress.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateQuestWithoutDueDate_When_DueDateOmitted()
    {
        // Given
        QuestTitle title = new("Open-ended quest");

        // When
        Quest quest = Quest.Create(title, "No deadline");

        // Then
        quest.DueDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleIsEmpty()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new QuestTitle(""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleExceedsMaxLength()
    {
        // Given
        string longTitle = new('x', 201);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => new QuestTitle(longTitle));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainTasks_When_TasksAreAdded()
    {
        // Given
        Quest quest = CreateQuest();
        TodoTask task1 = TodoTask.Create(new TaskTitle("Write abstract"));
        TodoTask task2 = TodoTask.Create(new TaskTitle("Create slide deck"));

        // When
        quest.AddTask(task1);
        quest.AddTask(task2);

        // Then
        quest.Tasks.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroProgress_When_NoTasksAreCompleted()
    {
        // Given
        Quest quest = CreateQuestWithTasks(5);

        // Then
        quest.Progress.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateProgress_When_SomeTasksAreCompleted()
    {
        // Given
        Quest quest = CreateQuestWithTasks(5);
        TodoTask firstTask = quest.Tasks[0];
        firstTask.MoveToInProgress();
        firstTask.MarkAsDone();

        // Then — 1 of 5 = 20%
        quest.Progress.ShouldBe(20);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveFullProgress_When_AllTasksAreCompleted()
    {
        // Given
        Quest quest = CreateQuestWithTasks(5);
        foreach (TodoTask task in quest.Tasks)
        {
            task.MoveToInProgress();
            task.MarkAsDone();
        }

        // Then
        quest.Progress.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveZeroProgress_When_QuestHasNoTasks()
    {
        // Given
        Quest quest = CreateQuest();

        // Then
        quest.Progress.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DuplicateTaskIsAdded()
    {
        // Given
        Quest quest = CreateQuest();
        TodoTask task = TodoTask.Create(new TaskTitle("Write abstract"));
        quest.AddTask(task);

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => quest.AddTask(task));
        ex.Message.ShouldContain("already");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DuplicateTaskIsAddedAmongMultiple()
    {
        // Given
        Quest quest = CreateQuest();
        TodoTask task1 = TodoTask.Create(new TaskTitle("First task"));
        TodoTask task2 = TodoTask.Create(new TaskTitle("Second task"));
        quest.AddTask(task1);
        quest.AddTask(task2);

        // When / Then — adding task1 again with multiple tasks already present
        DomainException ex = Should.Throw<DomainException>(() => quest.AddTask(task1));
        ex.Message.ShouldContain("already");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullTask()
    {
        // Given
        Quest quest = CreateQuest();

        // When / Then
        Should.Throw<ArgumentNullException>(() => quest.AddTask(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptQuestTitle_When_TitleIsExactlyMaxLength()
    {
        // Given
        string maxTitle = new('x', 200);

        // When
        QuestTitle title = new(maxTitle);

        // Then
        title.Value.ShouldBe(maxTitle);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleIsNull()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new QuestTitle(null!));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleIsWhitespaceOnly()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new QuestTitle("   "));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleIsControlCharactersOnly()
    {
        // Given / When / Then
        DomainException ex = Should.Throw<DomainException>(() => new QuestTitle("\x01\x02\x03"));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CalculateProgress_When_PartialTasksCompleted()
    {
        // Given — 3 of 4 tasks completed = 75%
        Quest quest = CreateQuestWithTasks(4);
        for (int i = 0; i < 3; i++)
        {
            quest.Tasks[i].MoveToInProgress();
            quest.Tasks[i].MarkAsDone();
        }

        // Then
        quest.Progress.ShouldBe(75);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveTask_When_TaskIsAssigned()
    {
        // Given
        Quest quest = CreateQuest();
        TodoTask task = TodoTask.Create(new TaskTitle("Removable"));
        quest.AddTask(task);

        // When
        quest.RemoveTask(task.Id);

        // Then
        quest.Tasks.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingUnassignedTask()
    {
        // Given
        Quest quest = CreateQuest();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(
            () => quest.RemoveTask(new TaskId(Guid.NewGuid())));
        ex.Message.ShouldContain("not assigned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RemovingNullTaskId()
    {
        // Given
        Quest quest = CreateQuest();

        // When / Then
        Should.Throw<ArgumentNullException>(() => quest.RemoveTask(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecalculateProgress_When_TaskRemoved()
    {
        // Given — 2 tasks, 1 done = 50%; remove the incomplete one → 100%
        Quest quest = CreateQuest();
        TodoTask done = TodoTask.Create(new TaskTitle("Done task"));
        done.MoveToInProgress();
        done.MarkAsDone();
        TodoTask incomplete = TodoTask.Create(new TaskTitle("Incomplete"));
        quest.AddTask(done);
        quest.AddTask(incomplete);
        quest.Progress.ShouldBe(50);

        // When
        quest.RemoveTask(incomplete.Id);

        // Then
        quest.Progress.ShouldBe(100);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Complete_When_AllTasksAreDone()
    {
        // Given
        Quest quest = CreateQuestWithTasks(2);
        foreach (TodoTask task in quest.Tasks)
        {
            task.MoveToInProgress();
            task.MarkAsDone();
        }

        // When / Then — should not throw
        Should.NotThrow(() => quest.Complete());
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SetIsCompleted_When_QuestCompleted()
    {
        // Given
        Quest quest = CreateQuestWithTasks(1);
        quest.Tasks[0].MoveToInProgress();
        quest.Tasks[0].MarkAsDone();

        // When
        quest.Complete();

        // Then
        quest.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingAlreadyCompletedQuest()
    {
        // Given
        Quest quest = CreateQuestWithTasks(1);
        quest.Tasks[0].MoveToInProgress();
        quest.Tasks[0].MarkAsDone();
        quest.Complete();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => quest.Complete());
        ex.Message.ShouldContain("already completed");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingWithNoTasks()
    {
        // Given
        Quest quest = CreateQuest();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => quest.Complete());
        ex.Message.ShouldContain("no tasks");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_CompletingWithIncompleteTasks()
    {
        // Given
        Quest quest = CreateQuestWithTasks(3);
        quest.Tasks[0].MoveToInProgress();
        quest.Tasks[0].MarkAsDone();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => quest.Complete());
        ex.Message.ShouldContain("not all tasks are done");
    }

    private static Quest CreateQuest()
    {
        return Quest.Create(
            new QuestTitle("Test quest"),
            "A test quest",
            new DateOnly(2026, 12, 31));
    }

    private static Quest CreateQuestWithTasks(int count)
    {
        Quest quest = CreateQuest();
        for (int i = 0; i < count; i++)
        {
            quest.AddTask(TodoTask.Create(new TaskTitle($"Task {i + 1}")));
        }

        return quest;
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RecalculateProgress_When_TaskDeletedFromQuest()
    {
        // Given — quest with 4 tasks, 2 completed. Delete one incomplete task.
        // Before: 2/4 = 50%. After removing 1 incomplete: 2/3 = 66%
        Quest quest = CreateQuestWithTasks(4);
        quest.Tasks[0].MoveToInProgress();
        quest.Tasks[0].MarkAsDone();
        quest.Tasks[1].MoveToInProgress();
        quest.Tasks[1].MarkAsDone();
        quest.Progress.ShouldBe(50);

        // When — delete the third task (incomplete)
        quest.RemoveTask(quest.Tasks[2].Id);

        // Then — progress recalculated: 2/3 = 66%
        quest.Progress.ShouldBe(66);
        quest.Tasks.Count.ShouldBe(3);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignToEpic_When_QuestHasNoEpic()
    {
        // Given
        Quest quest = CreateQuest();
        EpicId epicId = EpicId.New();

        // When
        quest.AssignToEpic(epicId);

        // Then
        quest.EpicId.ShouldBe(epicId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestAlreadyBelongsToAnEpic()
    {
        // Given — quest already assigned to an epic
        Quest quest = CreateQuest();
        quest.AssignToEpic(EpicId.New());

        // When / Then — attempting to assign to another epic
        DomainException ex = Should.Throw<DomainException>(() => quest.AssignToEpic(EpicId.New()));
        ex.Message.ShouldContain("already belongs to an epic");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AssigningNullEpicId()
    {
        // Given
        Quest quest = CreateQuest();

        // When / Then
        Should.Throw<ArgumentNullException>(() => quest.AssignToEpic(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnassignFromEpic_When_QuestBelongsToEpic()
    {
        // Given
        Quest quest = CreateQuest();
        quest.AssignToEpic(EpicId.New());

        // When
        quest.UnassignFromEpic();

        // Then
        quest.EpicId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UnassigningFromNoEpic()
    {
        // Given
        Quest quest = CreateQuest();

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => quest.UnassignFromEpic());
        ex.Message.ShouldContain("not assigned to any epic");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowReassignment_When_QuestUnassignedFirst()
    {
        // Given — quest assigned to first epic, then unassigned
        Quest quest = CreateQuest();
        EpicId firstEpicId = EpicId.New();
        EpicId secondEpicId = EpicId.New();
        quest.AssignToEpic(firstEpicId);
        quest.UnassignFromEpic();

        // When — assign to second epic
        quest.AssignToEpic(secondEpicId);

        // Then
        quest.EpicId.ShouldBe(secondEpicId);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullEpicId_When_QuestCreated()
    {
        // Given / When
        Quest quest = CreateQuest();

        // Then
        quest.EpicId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReplaceTask_When_TaskExistsInQuest()
    {
        // Given — quest with one task (at index 0)
        Quest quest = CreateQuest();
        TodoTask task = TodoTask.Create(new TaskTitle("Original"));
        quest.AddTask(task);
        quest.Tasks[0].Status.ShouldBe(TaskStatus.Todo);

        // When — replace with same task after status change
        task.MoveToInProgress();
        quest.ReplaceTask(task);

        // Then
        quest.Tasks[0].Status.ShouldBe(TaskStatus.InProgress);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ReplacingTaskNotInQuest()
    {
        // Given
        Quest quest = CreateQuest();
        TodoTask orphan = TodoTask.Create(new TaskTitle("Not in quest"));

        // When / Then
        DomainException ex = Should.Throw<DomainException>(() => quest.ReplaceTask(orphan));
        ex.Message.ShouldContain("is not assigned to this quest");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_ReplacingWithNull()
    {
        // Given
        Quest quest = CreateQuest();

        // When / Then
        Should.Throw<ArgumentNullException>(() => quest.ReplaceTask(null!));
    }
}
