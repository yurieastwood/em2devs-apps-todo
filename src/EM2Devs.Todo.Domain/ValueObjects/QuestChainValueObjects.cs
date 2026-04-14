namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed quest chain identifier.
/// </summary>
public sealed record QuestChainId(Guid Value)
{
    public static QuestChainId New() => new(Guid.NewGuid());
}

/// <summary>
/// A detected recurring quest pattern used to surface chain suggestions.
/// </summary>
public sealed record QuestChainPattern(QuestTitle Title, RecurrencePattern Cadence, int OccurrenceCount);

/// <summary>
/// Historical record of a completed quest used as input for pattern detection.
/// </summary>
public sealed record QuestCompletionRecord(QuestTitle Title, DateOnly CompletedOn);

/// <summary>
/// History entry capturing the outcome of a single chain instance.
/// </summary>
public sealed record QuestChainInstance(QuestId QuestId, DateOnly ScheduledOn, bool Completed, TimeSpan? TimeToComplete);
