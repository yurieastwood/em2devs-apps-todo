namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A short encouragement message sent between accountability partners.
/// Limited to 280 characters — not a full chat system.
/// </summary>
public sealed record CheckInMessage
{
    public const int MaxLength = 280;

    public string Text { get; }
    public DateTimeOffset SentAt { get; }
    public Guid SenderId { get; }

    public CheckInMessage(string text, DateTimeOffset sentAt, Guid senderId)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new Exceptions.DomainException("Check-in message text cannot be empty.");
        }

        if (text.Length > MaxLength)
        {
            throw new Exceptions.DomainException(
                $"Check-in message cannot exceed {MaxLength} characters.");
        }

        if (senderId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Sender ID cannot be empty.");
        }

        Text = text;
        SentAt = sentAt;
        SenderId = senderId;
    }
}
