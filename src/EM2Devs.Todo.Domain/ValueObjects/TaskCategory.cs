using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a task category used for estimation bias analysis.
/// Categories group tasks by type (e.g., "writing", "code review") so the system
/// can detect per-category estimation patterns.
/// </summary>
public sealed record TaskCategory
{
    public string Value { get; }

    private TaskCategory(string value)
    {
        Value = value;
    }

    public static TaskCategory From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Task category cannot be empty.");
        }

        return new TaskCategory(value.Trim().ToLowerInvariant());
    }
}
