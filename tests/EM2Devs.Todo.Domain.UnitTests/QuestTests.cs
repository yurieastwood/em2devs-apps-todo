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
}
