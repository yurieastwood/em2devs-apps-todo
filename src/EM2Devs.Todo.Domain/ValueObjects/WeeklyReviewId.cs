namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed weekly review identifier (ADR-0023).
/// </summary>
public sealed record WeeklyReviewId(Guid Value)
{
    public static WeeklyReviewId New() => new(Guid.NewGuid());
}
