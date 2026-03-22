namespace EM2Devs.Todo.Application.Ports;

public interface ICurrentUser
{
    Guid UserId { get; }
    string DisplayName { get; }
    bool IsAuthenticated { get; }
}
