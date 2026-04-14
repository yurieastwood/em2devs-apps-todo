using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Auth;
using EM2Devs.Todo.Infrastructure.Persistence;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class InMemoryUserRepositoryTests
{
    private static InMemoryUserRepository CreateRepo() => new(new BCryptPasswordHasher());

    [Fact]
    public async Task Should_SeedBothDemoUsers()
    {
        InMemoryUserRepository repo = CreateRepo();

        User? demo = await repo.GetByIdAsync(InMemoryUserRepository.DemoUserId);
        User? demo2 = await repo.GetByIdAsync(InMemoryUserRepository.Demo2UserId);

        demo.ShouldNotBeNull();
        demo.Email.ShouldBe("demo@waypoint.dev");
        demo.DisplayName.ShouldBe("Demo User");

        demo2.ShouldNotBeNull();
        demo2.Email.ShouldBe("demo2@waypoint.dev");
        demo2.DisplayName.ShouldBe("Demo User 2");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
    {
        InMemoryUserRepository repo = CreateRepo();

        User? upper = await repo.GetByEmailAsync("DEMO@WAYPOINT.DEV");
        User? mixed = await repo.GetByEmailAsync("Demo@Waypoint.Dev");

        upper.ShouldNotBeNull();
        mixed.ShouldNotBeNull();
        upper.Id.ShouldBe(InMemoryUserRepository.DemoUserId);
        mixed.Id.ShouldBe(InMemoryUserRepository.DemoUserId);
    }

    [Fact]
    public async Task SeededPasswordHash_ShouldVerifyAgainstKnownPlaintext()
    {
        BCryptPasswordHasher hasher = new();
        InMemoryUserRepository repo = new(hasher);

        User? demo = await repo.GetByEmailAsync("demo@waypoint.dev");

        demo.ShouldNotBeNull();
        hasher.Verify(InMemoryUserRepository.SeedPassword, demo.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistNewUser()
    {
        InMemoryUserRepository repo = CreateRepo();
        User newUser = User.Create(
            "new@waypoint.dev",
            "$2a$11$fakehashfakehashfakehashfakehashfakehashfakehashfake",
            "New User",
            DateTimeOffset.UtcNow);

        await repo.AddAsync(newUser);

        User? retrieved = await repo.GetByIdAsync(newUser.Id);
        retrieved.ShouldNotBeNull();
        retrieved.Email.ShouldBe("new@waypoint.dev");
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenUserAlreadyExists()
    {
        InMemoryUserRepository repo = CreateRepo();
        User duplicate = User.Create(
            "other@waypoint.dev",
            "$2a$11$fakehashfakehashfakehashfakehashfakehashfakehashfake",
            "Other",
            DateTimeOffset.UtcNow,
            InMemoryUserRepository.DemoUserId);

        await Should.ThrowAsync<InvalidOperationException>(() => repo.AddAsync(duplicate));
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_ForUnknownEmail()
    {
        InMemoryUserRepository repo = CreateRepo();

        User? missing = await repo.GetByEmailAsync("missing@waypoint.dev");

        missing.ShouldBeNull();
    }
}
