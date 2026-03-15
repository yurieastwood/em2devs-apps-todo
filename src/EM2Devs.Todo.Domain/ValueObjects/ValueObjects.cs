namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed task identifier (ADR-0002).
/// Prevents Guid/string confusion at compile time.
/// </summary>
public sealed record TaskId(Guid Value)
{
    public static TaskId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed recurring task identifier (ADR-0002).
/// </summary>
public sealed record RecurringTaskId(Guid Value)
{
    public static RecurringTaskId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed notification identifier (ADR-0002).
/// </summary>
public sealed record NotificationId(Guid Value)
{
    public static NotificationId New() => new(Guid.NewGuid());
}

/// <summary>
/// Validated task title (ADR-0002).
/// Enforces non-empty, max 200 characters on construction.
/// </summary>
public sealed record TaskTitle
{
    public string Value { get; }

    public TaskTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.All(c => char.IsWhiteSpace(c) || char.IsControl(c)))
        {
            throw new Exceptions.DomainException("Task title cannot be empty.");
        }

        if (value.EnumerateRunes().Count() > 200)
        {
            throw new Exceptions.DomainException("Task title cannot exceed 200 characters.");
        }

        Value = value;
    }
}
