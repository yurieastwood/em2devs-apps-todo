namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>Shared test constants for domain unit tests.</summary>
internal static class TestData
{
    /// <summary>
    /// Deterministic user id used by domain tests that don't care about multi-user —
    /// good enough for <see cref="EM2Devs.Todo.Domain.Entities.TodoTask.Create"/>
    /// which requires a non-empty <c>UserId</c>.
    /// </summary>
    public static readonly Guid TestUserId = new("11111111-1111-1111-1111-111111111111");
}
