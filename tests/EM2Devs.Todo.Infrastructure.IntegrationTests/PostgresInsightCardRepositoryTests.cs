using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class PostgresInsightCardRepositoryTests : IAsyncLifetime, IDisposable
{
    private static readonly Guid _userA = Guid.NewGuid();
    private static readonly Guid _userB = Guid.NewGuid();

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private TodoDbContext _dbContext = null!;
    private PostgresInsightCardRepository _repoA = null!;
    private PostgresInsightCardRepository _repoB = null!;

    public void Dispose() => _dbContext?.Dispose();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repoA = new PostgresInsightCardRepository(_dbContext, new FakeCurrentUser(_userA));
        _repoB = new PostgresInsightCardRepository(_dbContext, new FakeCurrentUser(_userB));
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    private static InsightCard NewCard()
        => InsightCard.Generate(InsightType.MorningProductivityPeak, "msg", "data", new DateOnly(2026, 5, 3), isValidated: true);

    [Fact]
    public async Task Should_PersistAndRetrieveCard_When_Added()
    {
        InsightCard card = NewCard();

        await _repoA.AddAsync(card);
        InsightCard? retrieved = await _repoA.GetByIdAsync(card.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(card.Id);
        retrieved.Message.ShouldBe("msg");
        retrieved.Status.ShouldBe(InsightCardStatus.Unread);
    }

    [Fact]
    public async Task Should_IsolateBetweenUsers_When_DifferentUsersAddCards()
    {
        InsightCard cardA = NewCard();
        await _repoA.AddAsync(cardA);

        (await _repoB.GetByIdAsync(cardA.Id)).ShouldBeNull();
        (await _repoA.GetByIdAsync(cardA.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_ExcludeDismissedCards_When_GetForCurrentUserCalled()
    {
        InsightCard card = NewCard();
        await _repoA.AddAsync(card);
        InsightCard fetched = (await _repoA.GetByIdAsync(card.Id))!;
        fetched.Dismiss();
        await _repoA.SaveAsync(fetched);

        IReadOnlyList<InsightCard> result = await _repoA.GetForCurrentUserAsync(includeRead: true);
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_ExcludeReadCards_When_IncludeReadIsFalse()
    {
        InsightCard card = NewCard();
        await _repoA.AddAsync(card);
        InsightCard fetched = (await _repoA.GetByIdAsync(card.Id))!;
        fetched.MarkAsRead();
        await _repoA.SaveAsync(fetched);

        IReadOnlyList<InsightCard> withReads = await _repoA.GetForCurrentUserAsync(includeRead: true);
        IReadOnlyList<InsightCard> withoutReads = await _repoA.GetForCurrentUserAsync(includeRead: false);

        withReads.Count.ShouldBe(1);
        withoutReads.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_PersistStatusChange_When_SaveCalledAfterMutation()
    {
        InsightCard card = NewCard();
        await _repoA.AddAsync(card);
        InsightCard fetched = (await _repoA.GetByIdAsync(card.Id))!;

        fetched.Save();
        await _repoA.SaveAsync(fetched);
        InsightCard? reloaded = await _repoA.GetByIdAsync(card.Id);

        reloaded!.Status.ShouldBe(InsightCardStatus.Saved);
    }
}
