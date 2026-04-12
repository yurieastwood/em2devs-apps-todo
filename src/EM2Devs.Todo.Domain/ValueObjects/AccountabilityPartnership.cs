namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Accountability partnership between two users.
/// Only one active partnership per user at a time.
/// Partners see daily summaries, not task-level detail.
/// </summary>
public sealed record AccountabilityPartnership
{
    public Guid RequesterId { get; }
    public Guid PartnerId { get; }
    public PartnershipStatus Status { get; }
    public DateOnly CreatedOn { get; }
    public DateOnly? EndedOn { get; }

    public AccountabilityPartnership(
        Guid requesterId,
        Guid partnerId,
        PartnershipStatus status,
        DateOnly createdOn,
        DateOnly? endedOn)
    {
        if (requesterId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Requester ID cannot be empty.");
        }

        if (partnerId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Partner ID cannot be empty.");
        }

        if (requesterId == partnerId)
        {
            throw new Exceptions.DomainException(
                "Cannot form a partnership with yourself.");
        }

        RequesterId = requesterId;
        PartnerId = partnerId;
        Status = status;
        CreatedOn = createdOn;
        EndedOn = endedOn;
    }

    public static AccountabilityPartnership Request(Guid requesterId, Guid partnerId, DateOnly today) =>
        new(requesterId, partnerId, PartnershipStatus.Pending, today, null);

    public AccountabilityPartnership Accept()
    {
        if (Status != PartnershipStatus.Pending)
        {
            throw new Exceptions.DomainException(
                "Only pending partnerships can be accepted.");
        }

        return new AccountabilityPartnership(
            RequesterId, PartnerId, PartnershipStatus.Active, CreatedOn, null);
    }

    public AccountabilityPartnership End(DateOnly today)
    {
        if (Status != PartnershipStatus.Active)
        {
            throw new Exceptions.DomainException(
                "Only active partnerships can be ended.");
        }

        return new AccountabilityPartnership(
            RequesterId, PartnerId, PartnershipStatus.Ended, CreatedOn, today);
    }

    public AccountabilityPartnership Decline()
    {
        if (Status != PartnershipStatus.Pending)
        {
            throw new Exceptions.DomainException(
                "Only pending partnerships can be declined.");
        }

        return new AccountabilityPartnership(
            RequesterId, PartnerId, PartnershipStatus.Ended, CreatedOn, null);
    }

    public bool InvolvesUser(Guid userId) =>
        RequesterId == userId || PartnerId == userId;

    public bool IsActive => Status == PartnershipStatus.Active;
    public bool IsPending => Status == PartnershipStatus.Pending;
}
