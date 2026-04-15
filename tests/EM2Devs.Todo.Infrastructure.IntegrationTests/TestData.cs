namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

internal static class TestData
{
    /// <summary>
    /// Deterministic user id used by infrastructure tests that don't care about multi-user.
    /// </summary>
    public static readonly Guid TestUserId = new("11111111-1111-1111-1111-111111111111");
}
