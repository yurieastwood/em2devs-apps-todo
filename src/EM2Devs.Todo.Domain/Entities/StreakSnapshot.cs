namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// End-of-day snapshot of streak state, written by DailyStreakEvaluationJob.
/// One row per day. Single-user demo mode: no UserId column yet.
/// </summary>
public sealed class StreakSnapshot
{
    public Guid Id { get; }
    public DateOnly SnapshotDate { get; }
    public int CurrentDays { get; }
    public int LongestDays { get; }
    public int GraceDaysAvailable { get; }
    public bool WasActive { get; }
    public DateTimeOffset CreatedAt { get; }

    private StreakSnapshot(
        Guid id,
        DateOnly snapshotDate,
        int currentDays,
        int longestDays,
        int graceDaysAvailable,
        bool wasActive,
        DateTimeOffset createdAt)
    {
        Id = id;
        SnapshotDate = snapshotDate;
        CurrentDays = currentDays;
        LongestDays = longestDays;
        GraceDaysAvailable = graceDaysAvailable;
        WasActive = wasActive;
        CreatedAt = createdAt;
    }

    public static StreakSnapshot Capture(
        DateOnly snapshotDate,
        int currentDays,
        int longestDays,
        int graceDaysAvailable,
        bool wasActive)
    {
        return new StreakSnapshot(
            id: Guid.NewGuid(),
            snapshotDate: snapshotDate,
            currentDays: currentDays,
            longestDays: longestDays,
            graceDaysAvailable: graceDaysAvailable,
            wasActive: wasActive,
            createdAt: DateTimeOffset.UtcNow);
    }
}
