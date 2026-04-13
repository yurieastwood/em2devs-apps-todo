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
/// Strongly-typed energy check-in identifier (ADR-0002).
/// </summary>
public sealed record EnergyCheckInId(Guid Value)
{
    public static EnergyCheckInId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed capacity snapshot identifier (ADR-0002).
/// </summary>
public sealed record CapacitySnapshotId(Guid Value)
{
    public static CapacitySnapshotId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed estimation record identifier (ADR-0002).
/// </summary>
public sealed record EstimationRecordId(Guid Value)
{
    public static EstimationRecordId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed daily brief identifier (ADR-0002).
/// </summary>
public sealed record DailyBriefId(Guid Value)
{
    public static DailyBriefId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed player profile identifier (ADR-0023).
/// </summary>
public sealed record PlayerProfileId(Guid Value)
{
    public static PlayerProfileId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed guild identifier (ADR-0023).
/// </summary>
public sealed record GuildId(Guid Value)
{
    public static GuildId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed guild quest identifier (ADR-0023).
/// </summary>
public sealed record GuildQuestId(Guid Value)
{
    public static GuildQuestId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed guild task identifier (ADR-0023).
/// </summary>
public sealed record GuildTaskId(Guid Value)
{
    public static GuildTaskId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed challenge identifier (ADR-0023).
/// </summary>
public sealed record ChallengeId(Guid Value)
{
    public static ChallengeId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed timeline event identifier (ADR-0023).
/// </summary>
public sealed record TimelineEventId(Guid Value)
{
    public static TimelineEventId New() => new(Guid.NewGuid());
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

        if (value.Any(c => char.IsControl(c)))
        {
            throw new Exceptions.DomainException("Task title cannot contain control characters.");
        }

        if (new System.Globalization.StringInfo(value).LengthInTextElements > 200)
        {
            throw new Exceptions.DomainException("Task title cannot exceed 200 characters.");
        }

        Value = value;
    }
}
