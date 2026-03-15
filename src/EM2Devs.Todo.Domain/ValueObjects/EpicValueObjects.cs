namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed epic identifier (ADR-0002).
/// </summary>
public sealed record EpicId(Guid Value)
{
    public static EpicId New() => new(Guid.NewGuid());
}

/// <summary>
/// Validated epic title (ADR-0002).
/// Enforces non-empty, max 200 characters on construction.
/// </summary>
public sealed record EpicTitle
{
    public string Value { get; }

    public EpicTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.All(c => char.IsWhiteSpace(c) || char.IsControl(c)))
        {
            throw new Exceptions.DomainException("Epic title cannot be empty.");
        }

        if (value.EnumerateRunes().Count() > 200)
        {
            throw new Exceptions.DomainException("Epic title cannot exceed 200 characters.");
        }

        Value = value;
    }
}
