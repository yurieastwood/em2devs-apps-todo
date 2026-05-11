using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class RegisterUserCommandHandlerTests
{
    private static readonly DateTimeOffset _fixedNow =
        new(2026, 4, 12, 9, 30, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => _fixedNow;
    }

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _tokens = Substitute.For<IJwtTokenService>();
    private readonly TimeProvider _clock = new FixedTimeProvider();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(_users, _hasher, _tokens, _clock);

        _hasher.Hash(Arg.Any<string>()).Returns(ci => $"hash::{ci.Arg<string>()}");
        _tokens.Issue(Arg.Any<User>()).Returns(
            new JwtToken("token-xyz", _fixedNow.AddHours(8)));
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnLoginResult_When_EmailNotTaken()
    {
        _users.GetByEmailAsync("alice@waypoint.dev", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new RegisterUserCommand("alice@waypoint.dev", "password123", "Alice");

        Result<LoginResult> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        LoginResult payload = result.Match(r => r, _ => null!);
        payload.Token.ShouldBe("token-xyz");
        payload.DisplayName.ShouldBe("Alice");
        payload.ExpiresAt.ShouldBe(_fixedNow.AddHours(8));
        payload.UserId.ShouldNotBe(Guid.Empty);

        await _users.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "alice@waypoint.dev"
                && u.PasswordHash == "hash::password123"
                && u.DisplayName == "Alice"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_EmailAlreadyRegistered()
    {
        var existing = User.Create("alice@waypoint.dev", "hash", "Alice", _fixedNow);
        _users.GetByEmailAsync("alice@waypoint.dev", Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new RegisterUserCommand("alice@waypoint.dev", "password123", "Alice");

        Result<LoginResult> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        _tokens.DidNotReceive().Issue(Arg.Any<User>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnValidationError_When_DomainRejectsInputs()
    {
        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Email missing '@' — passes pipeline validator gate (not invoked here)
        // but User.Create rejects, exercising the DomainException branch.
        var command = new RegisterUserCommand("not-an-email", "password123", "Alice");

        Result<LoginResult> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ValidationError>();
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            () => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflictError_When_EmailDeactivatedWithinHoldingPeriod()
    {
        var existing = User.Create("alice@waypoint.dev", "hash", "Alice", _fixedNow.AddDays(-10));
        existing.Deactivate(_fixedNow.AddDays(-10));
        _users.GetByEmailAsync("alice@waypoint.dev", Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new RegisterUserCommand("alice@waypoint.dev", "password123", "Alice");

        Result<LoginResult> result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _users.DidNotReceive().DeleteAsync(Arg.Any<EM2Devs.Todo.Domain.ValueObjects.UserId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReleaseEmail_When_HoldingPeriodElapsed()
    {
        var existing = User.Create(
            "alice@waypoint.dev", "hash", "Alice",
            _fixedNow.AddDays(-60));
        existing.Deactivate(_fixedNow.AddDays(-31));
        _users.GetByEmailAsync("alice@waypoint.dev", Arg.Any<CancellationToken>())
            .Returns(existing);

        var command = new RegisterUserCommand("alice@waypoint.dev", "password123", "Alice");

        Result<LoginResult> result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        await _users.Received(1).DeleteAsync(existing.Id, Arg.Any<CancellationToken>());
        await _users.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "alice@waypoint.dev" && !u.IsDeactivated),
            Arg.Any<CancellationToken>());
    }
}
