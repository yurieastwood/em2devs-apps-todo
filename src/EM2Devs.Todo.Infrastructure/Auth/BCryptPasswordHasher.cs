using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Infrastructure.Auth;

/// <summary>
/// <see cref="IPasswordHasher"/> implementation backed by the <c>BCrypt.Net-Next</c> package.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plaintext)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);
        return BCrypt.Net.BCrypt.HashPassword(plaintext);
    }

    public bool Verify(string plaintext, string hash)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);
        ArgumentException.ThrowIfNullOrEmpty(hash);
        return BCrypt.Net.BCrypt.Verify(plaintext, hash);
    }
}
