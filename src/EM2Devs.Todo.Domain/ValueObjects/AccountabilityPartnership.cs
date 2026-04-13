namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Accountability partnership between two users.
/// Only one active partnership per user at a time.
/// Partners see daily summaries, not task-level detail.
/// Requires minimum level 7 to send a request.
/// Check-in messages are limited to 5 per day per sender.
/// Partnerships are social only — they do not affect XP or progression.
/// </summary>
public sealed record AccountabilityPartnership
{
    public const int MinimumLevelRequired = 7;
    public const int MaxCheckInMessagesPerDay = 5;
    public const int MaxActivePartners = 1;

    public Guid RequesterId { get; }
    public Guid PartnerId { get; }
    public PartnershipStatus Status { get; }
    public DateOnly CreatedOn { get; }
    public DateOnly? EndedOn { get; }
    private readonly List<CheckInMessage> _messages;

    public IReadOnlyList<CheckInMessage> Messages => _messages.AsReadOnly();

    public AccountabilityPartnership(
        Guid requesterId,
        Guid partnerId,
        PartnershipStatus status,
        DateOnly createdOn,
        DateOnly? endedOn,
        IEnumerable<CheckInMessage>? messages = null)
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
        _messages = messages?.ToList() ?? [];
    }

    public static AccountabilityPartnership Request(
        Guid requesterId,
        Guid partnerId,
        DateOnly today,
        int requesterLevel) =>
        requesterLevel < MinimumLevelRequired
            ? throw new Exceptions.DomainException(
                $"Must be at least level {MinimumLevelRequired} to send partner requests.")
            : new(requesterId, partnerId, PartnershipStatus.Pending, today, null);

    public static AccountabilityPartnership Request(Guid requesterId, Guid partnerId, DateOnly today) =>
        new(requesterId, partnerId, PartnershipStatus.Pending, today, null);

    public static void ValidateCanSendRequest(
        IReadOnlyList<AccountabilityPartnership> existingPartnerships,
        Guid userId)
    {
        int activeCount = existingPartnerships.Count(p =>
            p.IsActive && p.InvolvesUser(userId));

        if (activeCount >= MaxActivePartners)
        {
            throw new Exceptions.DomainException(
                "You already have an active partner. End your current partnership before sending a new request.");
        }
    }

    public AccountabilityPartnership Accept()
    {
        if (Status != PartnershipStatus.Pending)
        {
            throw new Exceptions.DomainException(
                "Only pending partnerships can be accepted.");
        }

        return new AccountabilityPartnership(
            RequesterId, PartnerId, PartnershipStatus.Active, CreatedOn, null, _messages);
    }

    public AccountabilityPartnership End(DateOnly today)
    {
        if (Status != PartnershipStatus.Active)
        {
            throw new Exceptions.DomainException(
                "Only active partnerships can be ended.");
        }

        return new AccountabilityPartnership(
            RequesterId, PartnerId, PartnershipStatus.Ended, CreatedOn, today, _messages);
    }

    public AccountabilityPartnership Decline()
    {
        if (Status != PartnershipStatus.Pending)
        {
            throw new Exceptions.DomainException(
                "Only pending partnerships can be declined.");
        }

        return new AccountabilityPartnership(
            RequesterId, PartnerId, PartnershipStatus.Ended, CreatedOn, null, _messages);
    }

    public AccountabilityPartnership Dissolve(DateOnly today)
    {
        if (Status != PartnershipStatus.Active)
        {
            throw new Exceptions.DomainException(
                "Only active partnerships can be dissolved.");
        }

        return new AccountabilityPartnership(
            RequesterId, PartnerId, PartnershipStatus.Ended, CreatedOn, today, _messages);
    }

    public AccountabilityPartnership SendCheckInMessage(CheckInMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (Status != PartnershipStatus.Active)
        {
            throw new Exceptions.DomainException(
                "Can only send messages in an active partnership.");
        }

        if (!InvolvesUser(message.SenderId))
        {
            throw new Exceptions.DomainException(
                "Only partners in this partnership can send messages.");
        }

        DateOnly messageDate = DateOnly.FromDateTime(message.SentAt.UtcDateTime);
        int todayCount = _messages.Count(m =>
            m.SenderId == message.SenderId
            && DateOnly.FromDateTime(m.SentAt.UtcDateTime) == messageDate);

        if (todayCount >= MaxCheckInMessagesPerDay)
        {
            throw new Exceptions.DomainException(
                $"Cannot send more than {MaxCheckInMessagesPerDay} check-in messages per day.");
        }

        var updatedMessages = new List<CheckInMessage>(_messages) { message };
        return new AccountabilityPartnership(
            RequesterId, PartnerId, Status, CreatedOn, EndedOn, updatedMessages);
    }

    public bool InvolvesUser(Guid userId) =>
        RequesterId == userId || PartnerId == userId;

    public bool IsActive => Status == PartnershipStatus.Active;
    public bool IsPending => Status == PartnershipStatus.Pending;

    /// <summary>
    /// Both partners see the same status — this is enforced by the record
    /// being immutable and shared between both parties.
    /// </summary>
    public PartnershipStatus GetStatusForUser(Guid userId)
    {
        if (!InvolvesUser(userId))
        {
            throw new Exceptions.DomainException(
                "User is not part of this partnership.");
        }

        return Status;
    }

    public IReadOnlyList<CheckInMessage> GetMessagesForDay(DateOnly date) =>
        _messages.Where(m => DateOnly.FromDateTime(m.SentAt.UtcDateTime) == date).ToList();
}
