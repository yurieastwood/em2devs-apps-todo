namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed saga identifier (ADR-0023).
/// </summary>
public sealed record SagaId(Guid Value)
{
    public static SagaId New() => new(Guid.NewGuid());
}

/// <summary>
/// Validated saga title (ADR-0002).
/// Enforces non-empty, max 200 characters on construction.
/// </summary>
public sealed record SagaTitle
{
    public string Value { get; }

    public SagaTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.All(c => char.IsWhiteSpace(c) || char.IsControl(c)))
        {
            throw new Exceptions.DomainException("Saga title cannot be empty.");
        }

        if (new System.Globalization.StringInfo(value).LengthInTextElements > 200)
        {
            throw new Exceptions.DomainException("Saga title cannot exceed 200 characters.");
        }

        Value = value;
    }
}
