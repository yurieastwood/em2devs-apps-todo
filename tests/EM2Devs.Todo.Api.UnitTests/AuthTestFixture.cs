using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;

namespace EM2Devs.Todo.Api.UnitTests;

/// <summary>
/// Test helper that issues bearer tokens signed with the same dev JWT key
/// used in <c>appsettings.Development.json</c>, so <see cref="WebApplicationFactory{TEntryPoint}"/>
/// accepts them under the production authentication pipeline.
/// </summary>
internal static class AuthTestFixture
{
    /// <summary>Must match <c>Jwt:Key</c> in <c>appsettings.Development.json</c>.</summary>
    public const string SigningKey = "dev-only-signing-key-at-least-32-characters-long-for-hs256";

    /// <summary>Must match <c>Jwt:Issuer</c> in <c>appsettings.Development.json</c>.</summary>
    public const string Issuer = "waypoint-api";

    /// <summary>Must match <c>Jwt:Audience</c> in <c>appsettings.Development.json</c>.</summary>
    public const string Audience = "waypoint-web";

    /// <summary>Default user id used by tests that don't care about multi-user.</summary>
    public static readonly Guid DefaultUserId = new("00000000-0000-0000-0000-000000000001");

    /// <summary>Default display name used by tests.</summary>
    public const string DefaultDisplayName = "Demo User";

    /// <summary>Issue a signed JWT for the given user id.</summary>
    public static string GetTokenFor(
        Guid userId,
        string displayName = DefaultDisplayName,
        string email = "demo@waypoint.dev")
    {
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Name, displayName),
            new(JwtRegisteredClaimNames.Email, email),
        ];

        DateTime now = DateTime.UtcNow;
        JwtSecurityToken token = new(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Attach a bearer token for <see cref="DefaultUserId"/> to the given client's default headers.
    /// </summary>
    public static HttpClient Authenticated(this HttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", GetTokenFor(DefaultUserId));
        return client;
    }
}
