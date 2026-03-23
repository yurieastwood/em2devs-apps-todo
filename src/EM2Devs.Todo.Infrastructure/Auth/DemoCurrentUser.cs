using EM2Devs.Todo.Application.Ports;

namespace EM2Devs.Todo.Infrastructure.Auth;

public sealed class DemoCurrentUser : ICurrentUser
{
    public static readonly Guid DemoUserId = new("00000000-0000-0000-0000-000000000001");
    public const string DemoDisplayName = "Demo User";

    public Guid UserId { get; set; } = DemoUserId;
    public string DisplayName { get; set; } = DemoDisplayName;
    public bool IsAuthenticated { get; set; }
}
