namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Tracks a participant's state in a challenge, including task count, join time, and withdrawal status.
/// </summary>
public sealed record ChallengeParticipant
{
    public Guid UserId { get; }
    public int TasksCompleted { get; }
    public DateTimeOffset JoinedAt { get; }
    public bool Withdrawn { get; }

    /// <summary>
    /// Timestamp when the participant reached their current task count.
    /// Used for tie resolution: the participant who reached the count first ranks higher.
    /// </summary>
    public DateTimeOffset LastCompletedAt { get; }

    public ChallengeParticipant(Guid userId, int tasksCompleted, DateTimeOffset joinedAt, bool withdrawn = false, DateTimeOffset? lastCompletedAt = null)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Challenge participant user ID cannot be empty.");
        }

        if (tasksCompleted < 0)
        {
            throw new Exceptions.DomainException("Tasks completed cannot be negative.");
        }

        UserId = userId;
        TasksCompleted = tasksCompleted;
        JoinedAt = joinedAt;
        Withdrawn = withdrawn;
        LastCompletedAt = lastCompletedAt ?? joinedAt;
    }

    /// <summary>
    /// Records a task completion, incrementing the count and updating the timestamp.
    /// </summary>
    public ChallengeParticipant RecordTaskCompletion(DateTimeOffset completedAt)
    {
        if (Withdrawn)
        {
            throw new Exceptions.DomainException("Cannot record task completion for a withdrawn participant.");
        }

        return new ChallengeParticipant(UserId, TasksCompleted + 1, JoinedAt, false, completedAt);
    }

    /// <summary>
    /// Marks the participant as withdrawn. Progress is removed from leaderboard.
    /// </summary>
    public ChallengeParticipant Withdraw()
    {
        if (Withdrawn)
        {
            throw new Exceptions.DomainException("Participant has already withdrawn.");
        }

        return new ChallengeParticipant(UserId, TasksCompleted, JoinedAt, withdrawn: true, LastCompletedAt);
    }
}
