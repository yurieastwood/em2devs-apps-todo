using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Persistence;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class PostgresRecurringTaskRepositoryTests : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    private TodoDbContext _dbContext = null!;
    private PostgresRecurringTaskRepository _repository = null!;

    public void Dispose() => _dbContext?.Dispose();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new PostgresRecurringTaskRepository(_dbContext, new FakeCurrentUser(TestData.TestUserId));
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_PersistAndRetrieveRecurringTask_When_Created()
    {
        RecurringTask recurring = RecurringTask.Create(TestData.TestUserId, new TaskTitle("Daily standup"), RecurrencePattern.Daily);

        await _repository.SaveAsync(recurring);
        RecurringTask? retrieved = await _repository.GetByIdAsync(recurring.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(recurring.Id);
        retrieved.Title.Value.ShouldBe("Daily standup");
        retrieved.Pattern.ShouldBe(RecurrencePattern.Daily);
        retrieved.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_PersistTitleAndPatternUpdates_When_TemplateEdited()
    {
        RecurringTask recurring = RecurringTask.Create(TestData.TestUserId, new TaskTitle("Weekly review"), RecurrencePattern.Weekly);
        await _repository.SaveAsync(recurring);

        recurring.UpdateTitle(new TaskTitle("Weekly retro"));
        recurring.UpdatePattern(RecurrencePattern.Daily);
        await _repository.SaveAsync(recurring);

        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using TodoDbContext fresh = new(options);
        RecurringTask? reloaded = await fresh.RecurringTasks.FindAsync(recurring.Id);
        reloaded.ShouldNotBeNull();
        reloaded.Title.Value.ShouldBe("Weekly retro");
        reloaded.Pattern.ShouldBe(RecurrencePattern.Daily);
    }

    [Fact]
    public async Task Should_PersistPauseState_When_Paused()
    {
        RecurringTask recurring = RecurringTask.Create(TestData.TestUserId, new TaskTitle("Monthly invoice"), RecurrencePattern.Monthly);
        await _repository.SaveAsync(recurring);

        recurring.Pause();
        await _repository.SaveAsync(recurring);

        RecurringTask? reloaded = await _repository.GetByIdAsync(recurring.Id);
        reloaded.ShouldNotBeNull();
        reloaded.IsActive.ShouldBeFalse();
    }
}
