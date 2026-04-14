using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EM2Devs.Todo.Infrastructure.Auth;

/// <summary>
/// HS256 JWT issuer. Reads signing key, issuer, and audience from <see cref="IConfiguration"/>
/// under the <c>Jwt:*</c> section. Tokens expire 8 hours after issue.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    /// <summary>Default token lifetime.</summary>
    public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(IConfiguration configuration, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key configuration value is required.");
        _issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer configuration value is required.");
        _audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience configuration value is required.");
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public JwtToken Issue(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.Add(TokenLifetime);

        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Iat,
                now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
        ];

        JwtSecurityToken token = new(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtToken(jwt, expiresAt);
    }
}
