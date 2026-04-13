namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Status of a shared quest invitation.
/// </summary>
public enum SharedQuestInvitationStatus
{
    Pending,
    Accepted,
    Declined
}

/// <summary>
/// Strongly-typed shared quest invitation identifier (ADR-0023).
/// </summary>
public sealed record SharedQuestInvitationId(Guid Value)
{
    public static SharedQuestInvitationId New() => new(Guid.NewGuid());
}

/// <summary>
/// An invitation for a user to join a shared quest.
/// </summary>
public sealed record SharedQuestInvitation
{
    public SharedQuestInvitationId Id { get; }
    public SharedQuestId QuestId { get; }
    public Guid InviteeId { get; }
    public SharedQuestInvitationStatus Status { get; }

    public SharedQuestInvitation(SharedQuestInvitationId id, SharedQuestId questId, Guid inviteeId,
        SharedQuestInvitationStatus status = SharedQuestInvitationStatus.Pending)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        QuestId = questId ?? throw new ArgumentNullException(nameof(questId));

        if (inviteeId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Invitee ID cannot be empty.");
        }

        InviteeId = inviteeId;
        Status = status;
    }

    /// <summary>
    /// Accept the invitation.
    /// </summary>
    public SharedQuestInvitation Accept()
    {
        if (Status != SharedQuestInvitationStatus.Pending)
        {
            throw new Exceptions.DomainException("Only pending invitations can be accepted.");
        }

        return new SharedQuestInvitation(Id, QuestId, InviteeId, SharedQuestInvitationStatus.Accepted);
    }

    /// <summary>
    /// Decline the invitation.
    /// </summary>
    public SharedQuestInvitation Decline()
    {
        if (Status != SharedQuestInvitationStatus.Pending)
        {
            throw new Exceptions.DomainException("Only pending invitations can be declined.");
        }

        return new SharedQuestInvitation(Id, QuestId, InviteeId, SharedQuestInvitationStatus.Declined);
    }
}
