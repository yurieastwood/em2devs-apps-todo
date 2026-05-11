using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class RecoverAccountCommandHandlerTests
{
    private static readonly DateTimeOffset _now =
        new(2026, 4, 12, 9, 30, 0, TimeSpan.Zero);

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => _now;
    }

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _tokens = Substitute.For<IJwtTokenService>();
    private readonly TimeProvider _clock = new FixedTimeProvider();
    private readonly RecoverAccountCommandHandler _handler;

    public RecoverAccountCommandHandlerTests()
    {
        _handler = new RecoverAccountCommandHandler(_users, _hasher, _tokens, _clock);
    }

    private User ArrangeDeactivatedUser(DateTimeOffset deactivatedAt)
    {
        var id = new UserId(new Guid("33333333-3333-3333-3333-333333333333"));
        var user = User.Create("alice@waypoint.dev", "stored-hash", "Alice", _now.AddDays(-90), id);
        user.Deactivate(deactivatedAt);
        _users.GetByEmailAsync("alice@waypoint.dev", Arg.Any<CancellationToken>()).Returns(user);
        _tokens.Issue(user).Returns(new JwtToken("jwt-recover", _now.AddHours(8)));
        return user;
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReactivateAndReturnLoginResult_When_CredentialsValidAndWithinHoldingPeriod()
    {
        User user = ArrangeDeactivatedUser(_now.AddDays(-10));
        _hasher.Verify("password123", "stored-hash").Returns(true);

        Result<LoginResult> result = await _handler.Handle(
            new RecoverAccountCommand("alice@waypoint.dev", "password123"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        LoginResult payload = result.Match(r => r, _ => null!);
        payload.Token.ShouldBe("jwt-recover");
        payload.UserId.ShouldBe(user.Id.Value);
        user.IsDeactivated.ShouldBeFalse();
        await _users.Received(1).SaveAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnUnauthorized_When_UserNotFound()
    {
        _users.GetByEmailAsync("ghost@waypoint.dev", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        Result<LoginResult> result = await _handler.Handle(
            new RecoverAccountCommand("ghost@waypoint.dev", "password123"),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<UnauthorizedError>();
        _tokens.DidNotReceive().Issue(Arg.Any<User>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnUnauthorized_When_PasswordIncorrect()
    {
        ArrangeDeactivatedUser(_now.AddDays(-5));
        _hasher.Verify("wrong-password", "stored-hash").Returns(false);

        Result<LoginResult> result = await _handler.Handle(
            new RecoverAccountCommand("alice@waypoint.dev", "wrong-password"),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<UnauthorizedError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflict_When_AccountIsActive()
    {
        var id = new UserId(new Guid("44444444-4444-4444-4444-444444444444"));
        var user = User.Create("alice@waypoint.dev", "stored-hash", "Alice", _now.AddDays(-90), id);
        // Not deactivated.
        _users.GetByEmailAsync("alice@waypoint.dev", Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("password123", "stored-hash").Returns(true);

        Result<LoginResult> result = await _handler.Handle(
            new RecoverAccountCommand("alice@waypoint.dev", "password123"),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnConflict_When_HoldingPeriodElapsed()
    {
        ArrangeDeactivatedUser(_now.AddDays(-31));
        _hasher.Verify("password123", "stored-hash").Returns(true);

        Result<LoginResult> result = await _handler.Handle(
            new RecoverAccountCommand("alice@waypoint.dev", "password123"),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<ConflictError>();
        _tokens.DidNotReceive().Issue(Arg.Any<User>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            () => _handler.Handle(null!, CancellationToken.None));
    }
}
