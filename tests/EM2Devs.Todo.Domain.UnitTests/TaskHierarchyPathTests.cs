using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the TaskHierarchyPath value object
/// (breadcrumb: Saga &gt; Epic &gt; Quest &gt; Task).
/// </summary>
public sealed class TaskHierarchyPathTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BuildFullBreadcrumb_When_AllLevelsProvided()
    {
        TaskHierarchyPath path = new(
            TaskId.New(), new TaskTitle("Write unit tests"),
            QuestId.New(), new QuestTitle("Build authentication"),
            EpicId.New(), new EpicTitle("Launch MVP"),
            SagaId.New(), new SagaTitle("Launch my SaaS business"));

        path.Breadcrumb.ShouldBe("Launch my SaaS business > Launch MVP > Build authentication > Write unit tests");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OmitMissingLevels_When_OnlyQuestProvided()
    {
        TaskHierarchyPath path = new(
            TaskId.New(), new TaskTitle("Task"),
            QuestId.New(), new QuestTitle("Quest"));

        path.Breadcrumb.ShouldBe("Quest > Task");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_OmitAllHigherLevels_When_TaskStandalone()
    {
        TaskHierarchyPath path = new(TaskId.New(), new TaskTitle("Standalone"));
        path.Breadcrumb.ShouldBe("Standalone");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeEpicAndQuest_When_NoSaga()
    {
        TaskHierarchyPath path = new(
            TaskId.New(), new TaskTitle("T"),
            QuestId.New(), new QuestTitle("Q"),
            EpicId.New(), new EpicTitle("E"));

        path.Breadcrumb.ShouldBe("E > Q > T");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TaskIdIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TaskHierarchyPath(null!, new TaskTitle("t")));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TaskTitleIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new TaskHierarchyPath(TaskId.New(), null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestIdWithoutQuestTitle()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"), QuestId.New(), null));
        ex.Message.ShouldContain("quest title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_QuestTitleWithoutQuestId()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"), null, new QuestTitle("q")));
        ex.Message.ShouldContain("quest id");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicIdWithoutEpicTitle()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"),
                QuestId.New(), new QuestTitle("q"),
                EpicId.New(), null));
        ex.Message.ShouldContain("epic title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicTitleWithoutEpicId()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"),
                QuestId.New(), new QuestTitle("q"),
                null, new EpicTitle("e")));
        ex.Message.ShouldContain("epic id");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaIdWithoutSagaTitle()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"),
                QuestId.New(), new QuestTitle("q"),
                EpicId.New(), new EpicTitle("e"),
                SagaId.New(), null));
        ex.Message.ShouldContain("saga title");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaTitleWithoutSagaId()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"),
                QuestId.New(), new QuestTitle("q"),
                EpicId.New(), new EpicTitle("e"),
                null, new SagaTitle("s")));
        ex.Message.ShouldContain("saga id");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicWithoutQuest()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"),
                null, null,
                EpicId.New(), new EpicTitle("e")));
        ex.Message.ShouldContain("epic without a quest");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaWithoutEpic()
    {
        DomainException ex = Should.Throw<DomainException>(() =>
            new TaskHierarchyPath(TaskId.New(), new TaskTitle("t"),
                QuestId.New(), new QuestTitle("q"),
                null, null,
                SagaId.New(), new SagaTitle("s")));
        ex.Message.ShouldContain("saga without an epic");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExposeAllReferences()
    {
        TaskId taskId = TaskId.New();
        QuestId questId = QuestId.New();
        EpicId epicId = EpicId.New();
        SagaId sagaId = SagaId.New();
        TaskHierarchyPath path = new(
            taskId, new TaskTitle("t"),
            questId, new QuestTitle("q"),
            epicId, new EpicTitle("e"),
            sagaId, new SagaTitle("s"));

        path.TaskId.ShouldBe(taskId);
        path.QuestId.ShouldBe(questId);
        path.EpicId.ShouldBe(epicId);
        path.SagaId.ShouldBe(sagaId);
    }
}
