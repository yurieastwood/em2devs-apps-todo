using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Infrastructure.IntegrationTests;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(Guid userId) { UserId = userId; }
    public Guid UserId { get; }
    public string DisplayName => "Test";
    public bool IsAuthenticated => true;
}
