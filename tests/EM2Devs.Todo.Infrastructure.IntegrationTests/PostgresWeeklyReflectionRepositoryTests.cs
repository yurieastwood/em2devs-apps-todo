using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class PostgresWeeklyReflectionRepositoryTests : IAsyncLifetime, IDisposable
{
    private static readonly Guid _userA = Guid.NewGuid();
    private static readonly Guid _userB = Guid.NewGuid();

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private TodoDbContext _dbContext = null!;
    private PostgresWeeklyReflectionRepository _repoA = null!;
    private PostgresWeeklyReflectionRepository _repoB = null!;

    public void Dispose() => _dbContext?.Dispose();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repoA = new PostgresWeeklyReflectionRepository(_dbContext, new FakeCurrentUser(_userA));
        _repoB = new PostgresWeeklyReflectionRepository(_dbContext, new FakeCurrentUser(_userB));
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_ReturnNull_When_NoReflectionSavedForWeek()
    {
        WeeklyReflectionReadModel? result = await _repoA.GetAsync(new DateOnly(2026, 5, 3));
        result.ShouldBeNull();
    }

    [Fact]
    public async Task Should_PersistAndRetrieveReflection_When_Saved()
    {
        DateOnly week = new(2026, 5, 3);
        DateTimeOffset savedAt = new(2026, 5, 4, 10, 0, 0, TimeSpan.Zero);

        await _repoA.SaveAsync(week, "shipped feature", "code review delays", "block 2h focus", savedAt);
        WeeklyReflectionReadModel? result = await _repoA.GetAsync(week);

        result.ShouldNotBeNull();
        result.WhatWentWell.ShouldBe("shipped feature");
        result.WhatDragged.ShouldBe("code review delays");
        result.Adjustment.ShouldBe("block 2h focus");
        result.SavedAt.ShouldBe(savedAt);
    }

    [Fact]
    public async Task Should_ReplaceReflection_When_SavedTwiceForSameWeek()
    {
        DateOnly week = new(2026, 5, 3);
        await _repoA.SaveAsync(week, "v1", "v1", "v1", DateTimeOffset.UtcNow);

        await _repoA.SaveAsync(week, "v2", "v2", "v2", DateTimeOffset.UtcNow);

        WeeklyReflectionReadModel? result = await _repoA.GetAsync(week);
        result.ShouldNotBeNull();
        result.WhatWentWell.ShouldBe("v2");
    }

    [Fact]
    public async Task Should_IsolateBetweenUsers_When_TwoUsersSaveSameWeek()
    {
        DateOnly week = new(2026, 5, 3);
        await _repoA.SaveAsync(week, "a", "a", "a", DateTimeOffset.UtcNow);
        await _repoB.SaveAsync(week, "b", "b", "b", DateTimeOffset.UtcNow);

        WeeklyReflectionReadModel? a = await _repoA.GetAsync(week);
        WeeklyReflectionReadModel? b = await _repoB.GetAsync(week);

        a!.WhatWentWell.ShouldBe("a");
        b!.WhatWentWell.ShouldBe("b");
    }
}
