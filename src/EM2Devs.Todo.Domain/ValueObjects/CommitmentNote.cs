namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A commitment note attached to a rescheduled task, capturing the user's intent.
/// </summary>
public sealed record CommitmentNote
{
    public string Text { get; }
    public DateTimeOffset CreatedAt { get; }

    public CommitmentNote(string text, DateTimeOffset? createdAt = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new Exceptions.DomainException("Commitment note text cannot be empty.");
        }

        Text = text;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }
}
