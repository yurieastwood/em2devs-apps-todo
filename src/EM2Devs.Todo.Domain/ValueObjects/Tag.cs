using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Validated, normalised task tag. Tags are stored lowercase and trimmed
/// so that "Work", " work ", and "WORK" all compare as equal.
/// </summary>
public sealed record Tag
{
    public const int MaxLength = 50;

    public string Value { get; }

    private Tag(string value)
    {
        Value = value;
    }

    public static Tag From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Tag cannot be empty.");
        }

        string normalised = value.Trim().ToLowerInvariant();

        if (normalised.Length > MaxLength)
        {
            throw new DomainException($"Tag cannot exceed {MaxLength} characters.");
        }

        return new Tag(normalised);
    }
}
