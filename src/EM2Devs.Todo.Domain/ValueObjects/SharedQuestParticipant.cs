namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Role of a participant within a shared quest.
/// </summary>
public enum SharedQuestRole
{
    Creator,
    Participant
}

/// <summary>
/// A participant in a shared quest with their role and join date.
/// </summary>
public sealed record SharedQuestParticipant
{
    public Guid UserId { get; }
    public SharedQuestRole Role { get; }
    public DateOnly JoinedAt { get; }

    public SharedQuestParticipant(Guid userId, SharedQuestRole role, DateOnly joinedAt)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Participant user ID cannot be empty.");
        }

        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }
}
