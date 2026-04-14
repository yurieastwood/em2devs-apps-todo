namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Password hashing port. Implementations must use a slow, salted hash (e.g. bcrypt).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plaintext);
    bool Verify(string plaintext, string hash);
}
