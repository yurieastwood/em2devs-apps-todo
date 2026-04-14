using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the Saga entity and related value objects.
/// Encodes behaviours from quest-hierarchy.feature (Sagas rule).
/// </summary>
public sealed class SagaTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSaga_When_ValidDetailsProvided()
    {
        SagaTitle title = new("Launch my SaaS business");
        Saga saga = Saga.Create(title, "Go from idea to paying customers", "Sustainable product");

        saga.Id.Value.ShouldNotBe(Guid.Empty);
        saga.Title.ShouldBe(title);
        saga.Description.ShouldBe("Go from idea to paying customers");
        saga.Vision.ShouldBe("Sustainable product");
        saga.TargetDate.ShouldBeNull();
        saga.Progress.ShouldBe(0m);
        saga.Epics.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateSagaWithTargetDate_When_Provided()
    {
        DateOnly target = new(2027, 1, 1);
        Saga saga = Saga.Create(new SagaTitle("T"), "d", "v", target);
        saga.TargetDate.ShouldBe(target);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_CreatingSagaWithNullTitle()
    {
        Should.Throw<ArgumentNullException>(() => Saga.Create(null!, "d", "v"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaTitleIsEmpty()
    {
        DomainException ex = Should.Throw<DomainException>(() => new SagaTitle(""));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaTitleIsNull()
    {
        DomainException ex = Should.Throw<DomainException>(() => new SagaTitle(null!));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaTitleIsWhitespaceOnly()
    {
        DomainException ex = Should.Throw<DomainException>(() => new SagaTitle("   "));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaTitleIsControlCharactersOnly()
    {
        DomainException ex = Should.Throw<DomainException>(() => new SagaTitle("\x01\x02"));
        ex.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_SagaTitleExceedsMaxLength()
    {
        DomainException ex = Should.Throw<DomainException>(() => new SagaTitle(new string('x', 201)));
        ex.Message.ShouldContain("cannot exceed 200");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptSagaTitle_When_ExactlyMaxLength()
    {
        string max = new('x', 200);
        new SagaTitle(max).Value.ShouldBe(max);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateNewSagaId_WhenInvokingNew()
    {
        SagaId a = SagaId.New();
        SagaId b = SagaId.New();
        a.ShouldNotBe(b);
        a.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignEpicToSaga_When_AddedToSaga()
    {
        Saga saga = CreateSaga();
        Epic epic = Epic.Create(new EpicTitle("Launch MVP"), "d");

        saga.AddEpic(epic);

        saga.Epics.Count.ShouldBe(1);
        epic.SagaId.ShouldBe(saga.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainTwoEpics_When_BothAssigned()
    {
        Saga saga = CreateSaga();
        Epic e1 = Epic.Create(new EpicTitle("Launch MVP"), "d");
        Epic e2 = Epic.Create(new EpicTitle("Acquire users"), "d");

        saga.AddEpic(e1);
        saga.AddEpic(e2);

        saga.Epics.Count.ShouldBe(2);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AddingNullEpic()
    {
        Saga saga = CreateSaga();
        Should.Throw<ArgumentNullException>(() => saga.AddEpic(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DuplicateEpicIsAdded()
    {
        Saga saga = CreateSaga();
        Epic epic = Epic.Create(new EpicTitle("Epic"), "d");
        saga.AddEpic(epic);

        DomainException ex = Should.Throw<DomainException>(() => saga.AddEpic(epic));
        ex.Message.ShouldContain("already assigned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EpicBelongsToAnotherSaga()
    {
        Saga sagaA = CreateSaga();
        Saga sagaB = Saga.Create(new SagaTitle("Career growth"), "d", "v");
        Epic epic = Epic.Create(new EpicTitle("Launch MVP"), "d");
        sagaA.AddEpic(epic);

        DomainException ex = Should.Throw<DomainException>(() => sagaB.AddEpic(epic));
        ex.Message.ShouldContain("another saga");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AverageEpicProgress_When_MultipleEpicsAssigned()
    {
        // epic1 progress 100, epic2 progress 0 → saga 50
        Saga saga = CreateSaga();
        Epic e1 = Epic.Create(new EpicTitle("Done"), "d");
        Quest q1 = Quest.Create(new QuestTitle("q1"), "d");
        TodoTask t1 = TodoTask.Create(new TaskTitle("t1"));
        q1.AddTask(t1);
        t1.MoveToInProgress();
        t1.MarkAsDone();
        e1.AddQuest(q1);

        Epic e2 = Epic.Create(new EpicTitle("NotDone"), "d");
        Quest q2 = Quest.Create(new QuestTitle("q2"), "d");
        q2.AddTask(TodoTask.Create(new TaskTitle("t2")));
        e2.AddQuest(q2);

        saga.AddEpic(e1);
        saga.AddEpic(e2);

        saga.Progress.ShouldBe(50m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveEpic_When_EpicAssigned()
    {
        Saga saga = CreateSaga();
        Epic epic = Epic.Create(new EpicTitle("Removable"), "d");
        saga.AddEpic(epic);

        saga.RemoveEpic(epic.Id);

        saga.Epics.ShouldBeEmpty();
        epic.SagaId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_RemovingNullEpicId()
    {
        Saga saga = CreateSaga();
        Should.Throw<ArgumentNullException>(() => saga.RemoveEpic(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RemovingUnassignedEpic()
    {
        Saga saga = CreateSaga();
        DomainException ex = Should.Throw<DomainException>(() => saga.RemoveEpic(EpicId.New()));
        ex.Message.ShouldContain("not assigned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BuildTimeline_With_CompletedAndInProgressCounts()
    {
        Saga saga = CreateSaga();
        Epic done = CreateCompletedEpic("Done");
        Epic inProgress = Epic.Create(new EpicTitle("InProgress"), "d");
        Quest q = Quest.Create(new QuestTitle("q"), "d");
        q.AddTask(TodoTask.Create(new TaskTitle("t")));
        inProgress.AddQuest(q);

        saga.AddEpic(done);
        saga.AddEpic(inProgress);

        DateTimeOffset start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset now = start.AddDays(120);
        SagaTimeline timeline = saga.BuildTimeline(start, now);

        timeline.CompletedEpics.ShouldBe(1);
        timeline.InProgressEpics.ShouldBe(1);
        timeline.Progress.ShouldBe(50m);
        timeline.ProjectedCompletion.ShouldNotBeNull();
        // Rate = 1 / 120 per day, 1 remaining -> 120 more days from now
        timeline.ProjectedCompletion!.Value.ShouldBe(now.AddDays(120));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullTrajectory_When_NoCompletedEpics()
    {
        Saga saga = CreateSaga();
        Epic e = Epic.Create(new EpicTitle("e"), "d");
        saga.AddEpic(e);

        SagaTimeline t = saga.BuildTimeline(DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow);
        t.ProjectedCompletion.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullTrajectory_When_AllEpicsComplete()
    {
        Saga saga = CreateSaga();
        Epic e = CreateCompletedEpic("e");
        saga.AddEpic(e);

        SagaTimeline t = saga.BuildTimeline(DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow);
        t.ProjectedCompletion.ShouldBeNull();
        t.CompletedEpics.ShouldBe(1);
        t.InProgressEpics.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNullTrajectory_When_SagaEmpty()
    {
        Saga saga = CreateSaga();
        SagaTimeline t = saga.BuildTimeline(DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow);
        t.ProjectedCompletion.ShouldBeNull();
        t.CompletedEpics.ShouldBe(0);
        t.InProgressEpics.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_TimelineNowIsBeforeStart()
    {
        Saga saga = CreateSaga();
        DateTimeOffset start = DateTimeOffset.UtcNow;
        DomainException ex = Should.Throw<DomainException>(() => saga.BuildTimeline(start, start.AddSeconds(-1)));
        ex.Message.ShouldContain("cannot be before");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectEpicAssignedToAnotherSaga_OnEpicLevel()
    {
        Epic epic = Epic.Create(new EpicTitle("Launch MVP"), "d");
        epic.AssignToSaga(SagaId.New());

        DomainException ex = Should.Throw<DomainException>(() => epic.AssignToSaga(SagaId.New()));
        ex.Message.ShouldContain("already belongs");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_AssigningNullSagaIdToEpic()
    {
        Epic epic = Epic.Create(new EpicTitle("e"), "d");
        Should.Throw<ArgumentNullException>(() => epic.AssignToSaga(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UnassignEpicFromSaga_When_Assigned()
    {
        Epic epic = Epic.Create(new EpicTitle("e"), "d");
        SagaId sid = SagaId.New();
        epic.AssignToSaga(sid);

        epic.UnassignFromSaga();

        epic.SagaId.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_UnassigningEpicNotInSaga()
    {
        Epic epic = Epic.Create(new EpicTitle("e"), "d");
        DomainException ex = Should.Throw<DomainException>(() => epic.UnassignFromSaga());
        ex.Message.ShouldContain("not assigned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowTimelineWhen_NowEqualsStart()
    {
        // Boundary: now == startedAt is allowed (only strictly-before is rejected)
        Saga saga = CreateSaga();
        DateTimeOffset start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        SagaTimeline t = saga.BuildTimeline(start, start);
        t.CompletedEpics.ShouldBe(0);
        t.InProgressEpics.ShouldBe(0);
        t.ProjectedCompletion.ShouldBeNull();
    }

    private static Saga CreateSaga()
    {
        return Saga.Create(new SagaTitle("Test saga"), "desc", "vision");
    }

    private static Epic CreateCompletedEpic(string title)
    {
        Epic epic = Epic.Create(new EpicTitle(title), "d");
        Quest q = Quest.Create(new QuestTitle($"q-{title}"), "d");
        TodoTask t = TodoTask.Create(new TaskTitle($"t-{title}"));
        q.AddTask(t);
        t.MoveToInProgress();
        t.MarkAsDone();
        epic.AddQuest(q);
        epic.Complete();
        return epic;
    }
}
