using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class PostgresEpicRepositoryTests : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private TodoDbContext _dbContext = null!;
    private PostgresEpicRepository _repository = null!;
    private PostgresQuestRepository _questRepository = null!;
    private PostgresTaskRepository _taskRepository = null!;

    public void Dispose() => _dbContext?.Dispose();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        FakeCurrentUser user = new(TestUserId);
        _repository = new PostgresEpicRepository(_dbContext, user);
        _questRepository = new PostgresQuestRepository(_dbContext, user);
        _taskRepository = new PostgresTaskRepository(_dbContext, user);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_PersistAndRetrieveEpic_When_Saved()
    {
        Epic epic = Epic.Create(new EpicTitle("Q2 OKRs"), "objective alignment");

        await _repository.SaveAsync(epic);
        Epic? retrieved = await _repository.GetByIdAsync(epic.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Title.Value.ShouldBe("Q2 OKRs");
        retrieved.Description.ShouldBe("objective alignment");
        retrieved.IsCompleted.ShouldBeFalse();
        retrieved.Quests.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Should_HydrateQuestsByForeignKey_When_QuestsAssignedToEpic()
    {
        Epic epic = Epic.Create(new EpicTitle("E1"), "");
        await _repository.SaveAsync(epic);

        Quest quest = Quest.Create(new QuestTitle("Q1"), "");
        quest.AssignToEpic(epic.Id);
        await _questRepository.SaveAsync(quest);

        Epic? retrieved = await _repository.GetByIdAsync(epic.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Quests.Count.ShouldBe(1);
        retrieved.Quests[0].Id.ShouldBe(quest.Id);
    }

    [Fact]
    public async Task Should_HydrateNestedTasks_When_QuestsHaveAssignedTasks()
    {
        Epic epic = Epic.Create(new EpicTitle("E2"), "");
        await _repository.SaveAsync(epic);
        Quest quest = Quest.Create(new QuestTitle("Q2"), "");
        quest.AssignToEpic(epic.Id);
        await _questRepository.SaveAsync(quest);
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("nested"));
        task.AssignToQuest(quest.Id);
        await _taskRepository.SaveAsync(task);

        Epic? retrieved = await _repository.GetByIdAsync(epic.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Quests.Count.ShouldBe(1);
        retrieved.Quests[0].Tasks.Count.ShouldBe(1);
        retrieved.Quests[0].Tasks[0].Title.Value.ShouldBe("nested");
    }

    [Fact]
    public async Task Should_ReturnAllEpics_When_GetAllInvoked()
    {
        await _repository.SaveAsync(Epic.Create(new EpicTitle("a"), ""));
        await _repository.SaveAsync(Epic.Create(new EpicTitle("b"), ""));

        IReadOnlyList<Epic> all = await _repository.GetAllAsync();

        all.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Should_DeleteEpic_When_Exists()
    {
        Epic epic = Epic.Create(new EpicTitle("delete me"), "");
        await _repository.SaveAsync(epic);

        bool deleted = await _repository.DeleteAsync(epic.Id);

        deleted.ShouldBeTrue();
        (await _repository.GetByIdAsync(epic.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task Should_ReturnFalse_When_DeletingMissingEpic()
    {
        (await _repository.DeleteAsync(EpicId.New())).ShouldBeFalse();
    }

    [Fact]
    public async Task Should_ReturnNull_When_EpicDoesNotExist()
    {
        Epic? result = await _repository.GetByIdAsync(EpicId.New());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Should_NotPersistQuestsThroughEpicSave_When_EpicIsSaved()
    {
        Epic epic = Epic.Create(new EpicTitle("cascade guard"), "");
        Quest quest = Quest.Create(new QuestTitle("only via quest repo"), "");
        quest.AssignToEpic(epic.Id);
        epic.AddQuest(quest);

        await _repository.SaveAsync(epic);

        // Quest is in Epic's in-memory list but was NOT persisted via SaveAsync(epic).
        Quest? retrievedQuest = await _questRepository.GetByIdAsync(quest.Id);
        retrievedQuest.ShouldBeNull();
    }

    [Fact]
    public async Task Should_HydrateMultipleQuestsWithMultipleTasks_When_EpicIsRetrieved()
    {
        Epic epic = Epic.Create(new EpicTitle("multi"), "");
        await _repository.SaveAsync(epic);

        Quest q1 = Quest.Create(new QuestTitle("Q-A"), "");
        q1.AssignToEpic(epic.Id);
        await _questRepository.SaveAsync(q1);
        Quest q2 = Quest.Create(new QuestTitle("Q-B"), "");
        q2.AssignToEpic(epic.Id);
        await _questRepository.SaveAsync(q2);

        TodoTask t1 = TodoTask.Create(TestUserId, new TaskTitle("T1"));
        t1.AssignToQuest(q1.Id);
        TodoTask t2 = TodoTask.Create(TestUserId, new TaskTitle("T2"));
        t2.AssignToQuest(q1.Id);
        TodoTask t3 = TodoTask.Create(TestUserId, new TaskTitle("T3"));
        t3.AssignToQuest(q2.Id);
        await _taskRepository.SaveAsync(t1);
        await _taskRepository.SaveAsync(t2);
        await _taskRepository.SaveAsync(t3);

        Epic? retrieved = await _repository.GetByIdAsync(epic.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Quests.Count.ShouldBe(2);
        Quest rq1 = retrieved.Quests.Single(q => q.Id == q1.Id);
        Quest rq2 = retrieved.Quests.Single(q => q.Id == q2.Id);
        rq1.Tasks.Count.ShouldBe(2);
        rq2.Tasks.Count.ShouldBe(1);
    }
}
