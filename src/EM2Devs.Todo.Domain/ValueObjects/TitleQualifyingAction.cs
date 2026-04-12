namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a single qualifying action toward earning a title.
/// Each action occurs on a specific date and counts toward the title requirement.
/// </summary>
public sealed record TitleQualifyingAction
{
    public DateOnly OccurredOn { get; }

    public TitleQualifyingAction(DateOnly occurredOn)
    {
        OccurredOn = occurredOn;
    }
}
