using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Persistence;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class PostgresPlayerProfileRepositoryTests : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    private TodoDbContext _dbContext = null!;
    private PostgresPlayerProfileRepository _repository = null!;
    private LastXpBreakdownCache _breakdownCache = null!;

    public void Dispose() => _dbContext?.Dispose();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _breakdownCache = new LastXpBreakdownCache();
        _repository = new PostgresPlayerProfileRepository(
            _dbContext, _breakdownCache, new FakeCurrentUser(TestData.TestUserId));
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public FakeCurrentUser(Guid userId) { UserId = userId; }
        public Guid UserId { get; }
        public string DisplayName => "Test";
        public bool IsAuthenticated => true;
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_ReturnFreshProfile_When_NoRowExists()
    {
        PlayerProfileReadModel profile = await _repository.GetProfileAsync();

        profile.Level.ShouldBe(1);
        profile.TotalXp.ShouldBe(0);
        profile.CurrentStreak.ShouldBe(0);
        profile.LongestStreak.ShouldBe(0);
    }

    [Fact]
    public async Task Should_PersistAcrossInstances_When_StreakRecorded()
    {
        DateOnly today = new(2026, 4, 7);
        await _repository.RecordCompletionAsync(today);

        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        await using TodoDbContext fresh = new(options);
        var freshRepo = new PostgresPlayerProfileRepository(
            fresh, _breakdownCache, new FakeCurrentUser(TestData.TestUserId));

        PlayerProfileReadModel profile = await freshRepo.GetProfileAsync();

        profile.CurrentStreak.ShouldBe(1);
        profile.LongestStreak.ShouldBe(1);
    }

    [Fact]
    public async Task Should_IncrementCurrentAndLongest_When_StreakBuiltOverConsecutiveDays()
    {
        await _repository.RecordCompletionAsync(new DateOnly(2026, 4, 1));
        await _repository.RecordCompletionAsync(new DateOnly(2026, 4, 2));
        await _repository.RecordCompletionAsync(new DateOnly(2026, 4, 3));

        PlayerProfileReadModel profile = await _repository.GetProfileAsync();
        profile.CurrentStreak.ShouldBe(3);
        profile.LongestStreak.ShouldBe(3);
    }

    [Fact]
    public async Task Should_PreserveLongestStreak_When_StreakBreaksAndRestarts()
    {
        await _repository.RecordCompletionAsync(new DateOnly(2026, 4, 1));
        await _repository.RecordCompletionAsync(new DateOnly(2026, 4, 2));
        await _repository.RecordCompletionAsync(new DateOnly(2026, 4, 3));
        await _repository.RecordCompletionAsync(new DateOnly(2026, 4, 10));

        PlayerProfileReadModel profile = await _repository.GetProfileAsync();
        profile.CurrentStreak.ShouldBe(1);
        profile.LongestStreak.ShouldBe(3);
    }

    [Fact]
    public async Task Should_AwardXpAndPersistLevel_When_XpGranted()
    {
        await _repository.AwardXpAsync(new ExperiencePoints(60));

        PlayerProfileReadModel profile = await _repository.GetProfileAsync();
        profile.Level.ShouldBe(2);
    }
}
