using EM2Devs.Todo.Application.Commands;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Application.UnitTests.Commands;

public sealed class LoginCommandHandlerTests
{
    private static readonly DateTimeOffset _now = new(2026, 4, 12, 9, 30, 0, TimeSpan.Zero);

    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _tokens = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_users, _hasher, _tokens);
    }

    private User ArrangeExistingUser()
    {
        var id = new UserId(new Guid("11111111-1111-1111-1111-111111111111"));
        var user = User.Create("alice@waypoint.dev", "stored-hash", "Alice", _now, id);
        _users.GetByEmailAsync("alice@waypoint.dev", Arg.Any<CancellationToken>()).Returns(user);
        _tokens.Issue(user).Returns(new JwtToken("jwt-abc", _now.AddHours(8)));
        return user;
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnLoginResult_When_CredentialsValid()
    {
        User user = ArrangeExistingUser();
        _hasher.Verify("password123", "stored-hash").Returns(true);

        Result<LoginResult> result = await _handler.Handle(
            new LoginCommand("alice@waypoint.dev", "password123"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        LoginResult payload = result.Match(r => r, _ => null!);
        payload.Token.ShouldBe("jwt-abc");
        payload.UserId.ShouldBe(user.Id.Value);
        payload.DisplayName.ShouldBe("Alice");
        payload.ExpiresAt.ShouldBe(_now.AddHours(8));
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnUnauthorized_When_UserNotFound()
    {
        _users.GetByEmailAsync("ghost@waypoint.dev", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        Result<LoginResult> result = await _handler.Handle(
            new LoginCommand("ghost@waypoint.dev", "password123"),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<UnauthorizedError>();
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        _tokens.DidNotReceive().Issue(Arg.Any<User>());
    }

    [Fact]
    [Trait("Category", "Application")]
    public async Task Should_ReturnUnauthorized_When_PasswordIncorrect()
    {
        ArrangeExistingUser();
        _hasher.Verify("wrong-password", "stored-hash").Returns(false);

        Result<LoginResult> result = await _handler.Handle(
            new LoginCommand("alice@waypoint.dev", "wrong-password"),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<UnauthorizedError>();
        _tokens.DidNotReceive().Issue(Arg.Any<User>());
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
    public async Task Should_ReturnUnauthorized_When_AccountDeactivated()
    {
        User user = ArrangeExistingUser();
        user.Deactivate(_now);
        _hasher.Verify("password123", "stored-hash").Returns(true);

        Result<LoginResult> result = await _handler.Handle(
            new LoginCommand("alice@waypoint.dev", "password123"),
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.Match(_ => null!, e => e).ShouldBeOfType<UnauthorizedError>();
        _tokens.DidNotReceive().Issue(Arg.Any<User>());
    }
}
