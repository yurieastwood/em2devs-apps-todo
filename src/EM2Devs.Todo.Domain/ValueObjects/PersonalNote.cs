namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A personal note attached to a timeline event by the user.
/// Maps to: docs/features/reflection/journey-timeline.feature
/// Rule: "Users can browse, filter, and annotate their timeline"
/// </summary>
public sealed record PersonalNote
{
    public const int MaxLength = 500;

    public string Text { get; }
    public DateTimeOffset CreatedAt { get; }

    public PersonalNote(string text, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new Exceptions.DomainException("Personal note text cannot be empty.");
        }

        if (new System.Globalization.StringInfo(text).LengthInTextElements > MaxLength)
        {
            throw new Exceptions.DomainException($"Personal note cannot exceed {MaxLength} characters.");
        }

        Text = text;
        CreatedAt = createdAt;
    }
}
