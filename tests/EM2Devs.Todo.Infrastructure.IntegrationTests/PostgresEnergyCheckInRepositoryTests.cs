using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class PostgresEnergyCheckInRepositoryTests : IAsyncLifetime, IDisposable
{
    private static readonly Guid _userA = Guid.NewGuid();
    private static readonly Guid _userB = Guid.NewGuid();

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
    private TodoDbContext _dbContext = null!;
    private PostgresEnergyCheckInRepository _repoA = null!;
    private PostgresEnergyCheckInRepository _repoB = null!;

    public void Dispose() => _dbContext?.Dispose();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        DbContextOptions<TodoDbContext> options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        _dbContext = new TodoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _repoA = new PostgresEnergyCheckInRepository(_dbContext, new FakeCurrentUser(_userA));
        _repoB = new PostgresEnergyCheckInRepository(_dbContext, new FakeCurrentUser(_userB));
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_PersistAndRetrieveCheckIn_When_Added()
    {
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.High, DateTimeOffset.UtcNow);

        await _repoA.AddAsync(checkIn);
        IReadOnlyList<EnergyCheckIn> recent = await _repoA.GetRecentAsync();

        recent.Count.ShouldBe(1);
        recent[0].Id.ShouldBe(checkIn.Id);
        recent[0].Level.ShouldBe(EnergyLevel.High);
    }

    [Fact]
    public async Task Should_IsolateBetweenUsers_When_DifferentUsersAddCheckIns()
    {
        await _repoA.AddAsync(EnergyCheckIn.Create(EnergyLevel.Low, DateTimeOffset.UtcNow));

        (await _repoB.GetRecentAsync()).ShouldBeEmpty();
        (await _repoA.GetRecentAsync()).Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_FilterByDays_When_GetRecentCalled()
    {
        EnergyCheckIn old = EnergyCheckIn.Create(EnergyLevel.Medium, DateTimeOffset.UtcNow.AddDays(-100));
        EnergyCheckIn fresh = EnergyCheckIn.Create(EnergyLevel.High, DateTimeOffset.UtcNow);
        await _repoA.AddAsync(old);
        await _repoA.AddAsync(fresh);

        IReadOnlyList<EnergyCheckIn> last60 = await _repoA.GetRecentAsync(days: 60);

        last60.Count.ShouldBe(1);
        last60[0].Id.ShouldBe(fresh.Id);
    }

    [Fact]
    public async Task Should_ReturnTodayCheckIn_When_OneExistsForToday()
    {
        EnergyCheckIn today = EnergyCheckIn.Create(EnergyLevel.High, DateTimeOffset.UtcNow);
        await _repoA.AddAsync(today);

        EnergyCheckIn? result = await _repoA.GetTodayAsync();

        result.ShouldNotBeNull();
        result.Id.ShouldBe(today.Id);
    }

    [Fact]
    public async Task Should_ReturnNull_When_NoCheckInForToday()
    {
        await _repoA.AddAsync(EnergyCheckIn.Create(EnergyLevel.Low, DateTimeOffset.UtcNow.AddDays(-3)));

        (await _repoA.GetTodayAsync()).ShouldBeNull();
    }

    [Fact]
    public async Task Should_PersistLevelUpdate_When_UpdateAsyncCalled()
    {
        EnergyCheckIn checkIn = EnergyCheckIn.Create(EnergyLevel.Medium, DateTimeOffset.UtcNow);
        await _repoA.AddAsync(checkIn);
        EnergyCheckIn fetched = (await _repoA.GetRecentAsync()).Single();

        fetched.UpdateLevel(EnergyLevel.High, DateTimeOffset.UtcNow);
        await _repoA.UpdateAsync(fetched);
        EnergyCheckIn reloaded = (await _repoA.GetRecentAsync()).Single();

        reloaded.Level.ShouldBe(EnergyLevel.High);
        reloaded.HasFluctuated.ShouldBeTrue();
        reloaded.PreviousLevel.ShouldBe(EnergyLevel.Medium);
    }
}
