namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents the final results of a concluded challenge, including ranked participants and rewards.
/// Rankings are determined by tasks completed (descending), with ties broken by who reached the count first.
/// </summary>
public sealed record ChallengeResult
{
    /// <summary>XP awarded to all participants for participation.</summary>
    public const int ParticipationXp = 50;

    /// <summary>XP bonus for top-3 finishers, in addition to participation XP.</summary>
    public const int Top3BonusXp = 100;

    public IReadOnlyList<ChallengeRanking> Rankings { get; }
    public bool IsFinalized { get; }

    public ChallengeResult(IEnumerable<ChallengeRanking> rankings)
    {
        if (rankings is null)
        {
            throw new Exceptions.DomainException("Rankings cannot be null.");
        }

        Rankings = rankings.ToList().AsReadOnly();
        IsFinalized = true;
    }

    /// <summary>
    /// Creates challenge results from a list of active (non-withdrawn) participants.
    /// Ranks by tasks completed descending, then by who reached that count first (ascending).
    /// </summary>
    public static ChallengeResult FromParticipants(IEnumerable<ChallengeParticipant> participants)
    {
        if (participants is null)
        {
            throw new Exceptions.DomainException("Participants cannot be null.");
        }

        List<ChallengeParticipant> active = participants
            .Where(p => !p.Withdrawn)
            .OrderByDescending(p => p.TasksCompleted)
            .ThenBy(p => p.LastCompletedAt)
            .ToList();

        var rankings = new List<ChallengeRanking>();
        for (int i = 0; i < active.Count; i++)
        {
            ChallengeParticipant p = active[i];
            int rank = i + 1;
            bool receivesCosmetic = rank <= 3;
            int xpReward = rank <= 3 ? ParticipationXp + Top3BonusXp : ParticipationXp;

            rankings.Add(new ChallengeRanking(p.UserId, rank, p.TasksCompleted, xpReward, receivesCosmetic));
        }

        return new ChallengeResult(rankings);
    }
}

/// <summary>
/// A single participant's final ranking in a concluded challenge.
/// </summary>
public sealed record ChallengeRanking
{
    public Guid UserId { get; }
    public int Rank { get; }
    public int TasksCompleted { get; }
    public int XpReward { get; }
    public bool ReceivesCosmetic { get; }

    public ChallengeRanking(Guid userId, int rank, int tasksCompleted, int xpReward, bool receivesCosmetic)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Ranking user ID cannot be empty.");
        }

        if (rank < 1)
        {
            throw new Exceptions.DomainException("Rank must be at least 1.");
        }

        if (tasksCompleted < 0)
        {
            throw new Exceptions.DomainException("Tasks completed cannot be negative.");
        }

        if (xpReward < 0)
        {
            throw new Exceptions.DomainException("XP reward cannot be negative.");
        }

        UserId = userId;
        Rank = rank;
        TasksCompleted = tasksCompleted;
        XpReward = xpReward;
        ReceivesCosmetic = receivesCosmetic;
    }
}
