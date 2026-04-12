namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A guild member with role and join date.
/// </summary>
public sealed record GuildMember
{
    public Guid UserId { get; }
    public GuildRole Role { get; }
    public DateOnly JoinedOn { get; }

    public GuildMember(Guid userId, GuildRole role, DateOnly joinedOn)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Guild member user ID cannot be empty.");
        }

        UserId = userId;
        Role = role;
        JoinedOn = joinedOn;
    }
}
