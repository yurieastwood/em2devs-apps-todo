using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// End-of-day snapshot of streak state, written by DailyStreakEvaluationJob.
/// One row per (user, day). The unique index on (user_id, snapshot_date) keeps
/// concurrent writes idempotent.
/// </summary>
public sealed class StreakSnapshot
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public DateOnly SnapshotDate { get; }
    public int CurrentDays { get; }
    public int LongestDays { get; }
    public int GraceDaysAvailable { get; }
    public bool WasActive { get; }
    public DateTimeOffset CreatedAt { get; }

    private StreakSnapshot(
        Guid id,
        Guid userId,
        DateOnly snapshotDate,
        int currentDays,
        int longestDays,
        int graceDaysAvailable,
        bool wasActive,
        DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        SnapshotDate = snapshotDate;
        CurrentDays = currentDays;
        LongestDays = longestDays;
        GraceDaysAvailable = graceDaysAvailable;
        WasActive = wasActive;
        CreatedAt = createdAt;
    }

    public static StreakSnapshot Capture(
        Guid userId,
        DateOnly snapshotDate,
        int currentDays,
        int longestDays,
        int graceDaysAvailable,
        bool wasActive)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        return new StreakSnapshot(
            id: Guid.NewGuid(),
            userId: userId,
            snapshotDate: snapshotDate,
            currentDays: currentDays,
            longestDays: longestDays,
            graceDaysAvailable: graceDaysAvailable,
            wasActive: wasActive,
            createdAt: DateTimeOffset.UtcNow);
    }
}
