using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using EM2Devs.Todo.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class JwtTokenServiceTests
{
    private const string TestKey = "dev-signing-key-at-least-32-bytes-long-please";
    private const string TestIssuer = "waypoint-api";
    private const string TestAudience = "waypoint-web";

    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = TestKey,
            ["Jwt:Issuer"] = TestIssuer,
            ["Jwt:Audience"] = TestAudience,
        })
        .Build();

    private static User CreateUser() => User.Create(
        "alice@example.com",
        "hashed",
        "Alice",
        new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new UserId(new Guid("11111111-1111-1111-1111-111111111111")));

    [Fact]
    public void Issue_ShouldReturnToken_WithExpectedClaims()
    {
        JwtTokenService service = new(BuildConfig());
        User user = CreateUser();

        JwtToken token = service.Issue(user);

        token.Token.ShouldNotBeNullOrWhiteSpace();

        JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token.Token);
        parsed.Issuer.ShouldBe(TestIssuer);
        parsed.Audiences.ShouldContain(TestAudience);
        parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub)
            .Value.ShouldBe(user.Id.Value.ToString());
        parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name)
            .Value.ShouldBe(user.DisplayName);
        parsed.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email)
            .Value.ShouldBe(user.Email);
        parsed.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Iat);
    }

    [Fact]
    public void Issue_ShouldSetExpiryAbout8HoursFromNow()
    {
        JwtTokenService service = new(BuildConfig());
        User user = CreateUser();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        JwtToken token = service.Issue(user);

        TimeSpan delta = token.ExpiresAt - before;
        delta.ShouldBeGreaterThan(TimeSpan.FromHours(7.9));
        delta.ShouldBeLessThan(TimeSpan.FromHours(8.1));
    }

    [Fact]
    public void Issue_ShouldProduceSignatureValidatableWithConfiguredKey()
    {
        JwtTokenService service = new(BuildConfig());
        User user = CreateUser();

        JwtToken token = service.Issue(user);

        TokenValidationParameters parameters = new()
        {
            ValidIssuer = TestIssuer,
            ValidAudience = TestAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };

        JwtSecurityTokenHandler handler = new();
        Should.NotThrow(() => handler.ValidateToken(token.Token, parameters, out _));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenKeyMissing()
    {
        IConfiguration empty = new ConfigurationBuilder().AddInMemoryCollection([]).Build();

        Should.Throw<InvalidOperationException>(() => new JwtTokenService(empty));
    }
}
