using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Persistence;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

/// <summary>
/// Integration tests for PostgresTaskRepository using Testcontainers.
/// Each test gets a fresh database to ensure isolation.
/// </summary>
[Trait("Category", "Integration")]
public sealed class PostgresTaskRepositoryTests : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private TodoDbContext _dbContext = null!;
    private PostgresTaskRepository _repository = null!;

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repository = new PostgresTaskRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_PersistAndRetrieveTask_When_TaskIsCreated()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Buy groceries"));

        // When
        await _repository.SaveAsync(task);
        TodoTask? retrieved = await _repository.GetByIdAsync(task.Id);

        // Then
        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(task.Id);
        retrieved.Title.Value.ShouldBe("Buy groceries");
        retrieved.Status.ShouldBe(Domain.TaskStatus.Todo);
    }

    [Fact]
    public async Task Should_ReturnAllTasks_When_MultiplTasksExist()
    {
        // Given
        TodoTask task1 = TodoTask.Create(new TaskTitle("Task one"));
        TodoTask task2 = TodoTask.Create(new TaskTitle("Task two"));
        await _repository.SaveAsync(task1);
        await _repository.SaveAsync(task2);

        // When
        IReadOnlyList<TodoTask> tasks = await _repository.GetAllAsync();

        // Then
        tasks.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Should_PersistStatusChange_When_TaskIsCompleted()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Write report"));
        await _repository.SaveAsync(task);

        // When
        task.MoveToInProgress();
        await _repository.SaveAsync(task);
        task.MarkAsDone();
        await _repository.SaveAsync(task);

        // Reload from database
        TodoTask? retrieved = await CreateFreshContext()
            .Tasks.FindAsync(task.Id);

        // Then
        retrieved.ShouldNotBeNull();
        retrieved.Status.ShouldBe(Domain.TaskStatus.Done);
    }

    [Fact]
    public async Task Should_DeleteTask_When_TaskExists()
    {
        // Given
        TodoTask task = TodoTask.Create(new TaskTitle("Delete me"));
        await _repository.SaveAsync(task);

        // When
        bool deleted = await _repository.DeleteAsync(task.Id);

        // Then
        deleted.ShouldBeTrue();
        TodoTask? retrieved = await _repository.GetByIdAsync(task.Id);
        retrieved.ShouldBeNull();
    }

    [Fact]
    public async Task Should_ReturnFalse_When_DeletingNonexistentTask()
    {
        // When
        bool deleted = await _repository.DeleteAsync(TaskId.New());

        // Then
        deleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_ReturnNull_When_TaskDoesNotExist()
    {
        // When
        TodoTask? retrieved = await _repository.GetByIdAsync(TaskId.New());

        // Then
        retrieved.ShouldBeNull();
    }

    [Fact]
    public async Task Should_ReturnEmptyList_When_NoTasksExist()
    {
        // When
        IReadOnlyList<TodoTask> tasks = await _repository.GetAllAsync();

        // Then
        tasks.ShouldBeEmpty();
    }

    private TodoDbContext CreateFreshContext()
    {
        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new TodoDbContext(options);
    }
}
