using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class PostgresQuestRepositoryTests : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private TodoDbContext _dbContext = null!;
    private PostgresQuestRepository _repository = null!;
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
        _repository = new PostgresQuestRepository(_dbContext, user);
        _taskRepository = new PostgresTaskRepository(_dbContext, user);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_PersistAndRetrieveQuest_When_Saved()
    {
        Quest quest = Quest.Create(new QuestTitle("Reach inbox zero"), "weekly target");

        await _repository.SaveAsync(quest);
        Quest? retrieved = await _repository.GetByIdAsync(quest.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(quest.Id);
        retrieved.Title.Value.ShouldBe("Reach inbox zero");
        retrieved.Description.ShouldBe("weekly target");
        retrieved.IsCompleted.ShouldBeFalse();
        retrieved.Tasks.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Should_HydrateTasksByForeignKey_When_TasksAssignedToQuest()
    {
        Quest quest = Quest.Create(new QuestTitle("Ship release"), "v1");
        await _repository.SaveAsync(quest);

        TodoTask t1 = TodoTask.Create(TestUserId, new TaskTitle("draft notes"));
        t1.AssignToQuest(quest.Id);
        TodoTask t2 = TodoTask.Create(TestUserId, new TaskTitle("publish blog"));
        t2.AssignToQuest(quest.Id);
        await _taskRepository.SaveAsync(t1);
        await _taskRepository.SaveAsync(t2);

        Quest? retrieved = await _repository.GetByIdAsync(quest.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Tasks.Count.ShouldBe(2);
        retrieved.Tasks.Select(t => t.Title.Value).ShouldBe(["draft notes", "publish blog"], ignoreOrder: true);
    }

    [Fact]
    public async Task Should_NotPersistTasksThroughQuestSave_When_QuestIsSaved()
    {
        Quest quest = Quest.Create(new QuestTitle("test isolation"), "x");
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("only via task repo"));
        quest.AddTask(task);

        await _repository.SaveAsync(quest);

        // Task is in Quest's in-memory list but was NOT persisted via SaveAsync(quest).
        TodoTask? retrievedTask = await _taskRepository.GetByIdAsync(task.Id);
        retrievedTask.ShouldBeNull();
    }

    [Fact]
    public async Task Should_ReturnAllQuests_When_GetAllInvoked()
    {
        await _repository.SaveAsync(Quest.Create(new QuestTitle("a"), ""));
        await _repository.SaveAsync(Quest.Create(new QuestTitle("b"), ""));

        IReadOnlyList<Quest> all = await _repository.GetAllAsync();

        all.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Should_FindQuestsByTaskId_When_TaskIsAssigned()
    {
        Quest quest = Quest.Create(new QuestTitle("findable"), "");
        await _repository.SaveAsync(quest);
        TodoTask task = TodoTask.Create(TestUserId, new TaskTitle("hook"));
        task.AssignToQuest(quest.Id);
        await _taskRepository.SaveAsync(task);

        IReadOnlyList<Quest> matches = await _repository.GetByTaskIdAsync(task.Id);

        matches.Count.ShouldBe(1);
        matches[0].Id.ShouldBe(quest.Id);
    }

    [Fact]
    public async Task Should_DeleteQuest_When_Exists()
    {
        Quest quest = Quest.Create(new QuestTitle("to be deleted"), "");
        await _repository.SaveAsync(quest);

        bool deleted = await _repository.DeleteAsync(quest.Id);

        deleted.ShouldBeTrue();
        (await _repository.GetByIdAsync(quest.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task Should_ReturnFalse_When_DeletingMissingQuest()
    {
        bool deleted = await _repository.DeleteAsync(QuestId.New());

        deleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_ReturnNull_When_QuestDoesNotExist()
    {
        Quest? result = await _repository.GetByIdAsync(QuestId.New());

        result.ShouldBeNull();
    }
}
