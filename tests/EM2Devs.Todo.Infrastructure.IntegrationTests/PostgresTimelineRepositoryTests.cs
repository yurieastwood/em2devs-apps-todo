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
public sealed class PostgresTimelineRepositoryTests : IAsyncLifetime, IDisposable
{
    private static readonly Guid _userA = Guid.NewGuid();
    private static readonly Guid _userB = Guid.NewGuid();

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private TodoDbContext _dbContext = null!;
    private PostgresTimelineRepository _repoA = null!;
    private PostgresTimelineRepository _repoB = null!;

    public void Dispose() => _dbContext?.Dispose();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repoA = new PostgresTimelineRepository(_dbContext, new FakeCurrentUser(_userA));
        _repoB = new PostgresTimelineRepository(_dbContext, new FakeCurrentUser(_userB));
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_PersistAndRetrieveEvent_When_Added()
    {
        TimelineEvent ev = TimelineEvent.Create(TimelineEventType.QuestCompleted, DateTimeOffset.UtcNow, "shipped Q1");

        await _repoA.AddAsync(ev);
        IReadOnlyList<TimelineEvent> events = await _repoA.GetEventsAsync();

        events.Count.ShouldBe(1);
        events[0].Id.ShouldBe(ev.Id);
        events[0].Details.ShouldBe("shipped Q1");
        events[0].EventType.ShouldBe(TimelineEventType.QuestCompleted);
    }

    [Fact]
    public async Task Should_PersistNote_When_EventHasNote()
    {
        TimelineEvent ev = TimelineEvent.Create(TimelineEventType.QuestCompleted, DateTimeOffset.UtcNow, "details");
        ev.AddNote(new PersonalNote("proud of this one", DateTimeOffset.UtcNow));

        await _repoA.AddAsync(ev);
        TimelineEvent reloaded = (await _repoA.GetEventsAsync()).Single();

        reloaded.Note.ShouldNotBeNull();
        reloaded.Note!.Text.ShouldBe("proud of this one");
    }

    [Fact]
    public async Task Should_IsolateBetweenUsers_When_DifferentUsersAddEvents()
    {
        await _repoA.AddAsync(TimelineEvent.Create(TimelineEventType.QuestCompleted, DateTimeOffset.UtcNow, "a"));

        (await _repoB.GetEventsAsync()).ShouldBeEmpty();
        (await _repoA.GetEventsAsync()).Count.ShouldBe(1);
    }
}
