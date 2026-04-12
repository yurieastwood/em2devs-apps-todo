namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed quest identifier (ADR-0002).
/// </summary>
public sealed record QuestId(Guid Value)
{
    public static QuestId New() => new(Guid.NewGuid());
}

/// <summary>
/// Validated quest title (ADR-0002).
/// Enforces non-empty, max 200 characters on construction.
/// </summary>
public sealed record QuestTitle
{
    public string Value { get; }

    public QuestTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.All(c => char.IsWhiteSpace(c) || char.IsControl(c)))
        {
            throw new Exceptions.DomainException("Quest title cannot be empty.");
        }

        if (new System.Globalization.StringInfo(value).LengthInTextElements > 200)
        {
            throw new Exceptions.DomainException("Quest title cannot exceed 200 characters.");
        }

        Value = value;
    }
}
