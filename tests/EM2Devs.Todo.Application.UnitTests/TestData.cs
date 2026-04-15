using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Application.UnitTests;

internal static class TestData
{
    /// <summary>
    /// Deterministic user id used by application tests that don't care about multi-user.
    /// </summary>
    public static readonly Guid TestUserId = new("11111111-1111-1111-1111-111111111111");
}

/// <summary>
/// Minimal <see cref="ICurrentUser"/> test double for wiring into repositories
/// that inject the real JWT-backed current user in production.
/// </summary>
internal sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }

    public string DisplayName => "Test";

    public bool IsAuthenticated => true;
}
