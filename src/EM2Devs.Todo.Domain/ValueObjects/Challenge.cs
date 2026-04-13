namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a time-limited competition (challenge) that users can opt into.
/// Challenges are either Global (system-generated) or Guild (created by any guild member).
/// Non-participation carries no penalty. Anti-gaming measures ensure fair competition.
/// </summary>
public sealed record Challenge
{
    public const int MinTitleLength = 5;
    public const int MaxTitleLength = 200;

    private readonly List<ChallengeParticipant> _participants;

    public ChallengeId Id { get; }
    public string Title { get; }
    public ChallengeType Type { get; }
    public DateTimeOffset StartTime { get; }
    public DateTimeOffset EndTime { get; }
    public string Objective { get; }
    public string Reward { get; }
    public GuildId? GuildId { get; }
    public Guid? CreatedByUserId { get; }
    public ChallengeResult? Result { get; }
    public IReadOnlyList<ChallengeParticipant> Participants => _participants.AsReadOnly();

    public Challenge(
        ChallengeId id,
        string title,
        ChallengeType type,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string objective,
        string reward,
        IEnumerable<ChallengeParticipant>? participants = null,
        GuildId? guildId = null,
        Guid? createdByUserId = null,
        ChallengeResult? result = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new Exceptions.DomainException("Challenge title cannot be empty.");
        }

        if (title.Length > MaxTitleLength)
        {
            throw new Exceptions.DomainException($"Challenge title cannot exceed {MaxTitleLength} characters.");
        }

        if (endTime <= startTime)
        {
            throw new Exceptions.DomainException("Challenge end time must be after start time.");
        }

        if (string.IsNullOrWhiteSpace(objective))
        {
            throw new Exceptions.DomainException("Challenge objective cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(reward))
        {
            throw new Exceptions.DomainException("Challenge reward cannot be empty.");
        }

        if (type == ChallengeType.Guild && guildId is null)
        {
            throw new Exceptions.DomainException("Guild challenges must have a guild ID.");
        }

        if (type == ChallengeType.Global && createdByUserId is not null)
        {
            throw new Exceptions.DomainException("Global challenges are system-generated and cannot have a creator user ID.");
        }

        if (type == ChallengeType.Guild && createdByUserId is null)
        {
            throw new Exceptions.DomainException("Guild challenges must have a creator user ID.");
        }

        Title = title;
        Type = type;
        StartTime = startTime;
        EndTime = endTime;
        Objective = objective;
        Reward = reward;
        GuildId = guildId;
        CreatedByUserId = createdByUserId;
        Result = result;
        _participants = participants?.ToList() ?? [];
    }

    /// <summary>
    /// Creates a new system-generated global challenge.
    /// </summary>
    public static Challenge CreateGlobal(
        string title,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string objective,
        string reward)
    {
        return new Challenge(
            ChallengeId.New(),
            title,
            ChallengeType.Global,
            startTime,
            endTime,
            objective,
            reward);
    }

    /// <summary>
    /// Creates a guild challenge. Any guild member can create one.
    /// </summary>
    public static Challenge CreateGuild(
        string title,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        string objective,
        string reward,
        GuildId guildId,
        Guid createdByUserId)
    {
        return new Challenge(
            ChallengeId.New(),
            title,
            ChallengeType.Guild,
            startTime,
            endTime,
            objective,
            reward,
            guildId: guildId,
            createdByUserId: createdByUserId);
    }

    /// <summary>
    /// Whether the challenge is currently active (within the time window).
    /// </summary>
    public bool IsActive(DateTimeOffset now)
    {
        return now >= StartTime && now <= EndTime && Result is null;
    }

    /// <summary>
    /// Whether the challenge period has ended.
    /// </summary>
    public bool HasEnded(DateTimeOffset now)
    {
        return now > EndTime;
    }

    /// <summary>
    /// Register a user as a participant in this challenge.
    /// </summary>
    public Challenge Join(Guid userId, DateTimeOffset now)
    {
        if (!IsActive(now))
        {
            throw new Exceptions.DomainException("Cannot join a challenge that is not active.");
        }

        if (_participants.Exists(p => p.UserId == userId))
        {
            throw new Exceptions.DomainException("User is already a participant in this challenge.");
        }

        var participant = new ChallengeParticipant(userId, 0, now);
        List<ChallengeParticipant> updated = [.. _participants, participant];
        return new Challenge(Id, Title, Type, StartTime, EndTime, Objective, Reward, updated, GuildId, CreatedByUserId, Result);
    }

    /// <summary>
    /// Records a qualifying task completion for a participant.
    /// </summary>
    public Challenge RecordTaskCompletion(Guid userId, DateTimeOffset completedAt)
    {
        ChallengeParticipant? participant = _participants.Find(p => p.UserId == userId);
        if (participant is null)
        {
            throw new Exceptions.DomainException("User is not a participant in this challenge.");
        }

        ChallengeParticipant updated = participant.RecordTaskCompletion(completedAt);
        List<ChallengeParticipant> updatedList = _participants
            .Select(p => p.UserId == userId ? updated : p)
            .ToList();

        return new Challenge(Id, Title, Type, StartTime, EndTime, Objective, Reward, updatedList, GuildId, CreatedByUserId, Result);
    }

    /// <summary>
    /// Withdraw a participant from the challenge. Their results are forfeited.
    /// </summary>
    public Challenge Withdraw(Guid userId)
    {
        ChallengeParticipant? participant = _participants.Find(p => p.UserId == userId);
        if (participant is null)
        {
            throw new Exceptions.DomainException("User is not a participant in this challenge.");
        }

        ChallengeParticipant updated = participant.Withdraw();
        List<ChallengeParticipant> updatedList = _participants
            .Select(p => p.UserId == userId ? updated : p)
            .ToList();

        return new Challenge(Id, Title, Type, StartTime, EndTime, Objective, Reward, updatedList, GuildId, CreatedByUserId, Result);
    }

    /// <summary>
    /// Conclude the challenge and calculate final results.
    /// Rankings determined by tasks completed, with ties broken by who reached the count first.
    /// </summary>
    public Challenge Conclude()
    {
        if (Result is not null)
        {
            throw new Exceptions.DomainException("Challenge has already been concluded.");
        }

        ChallengeResult result = ChallengeResult.FromParticipants(_participants);
        return new Challenge(Id, Title, Type, StartTime, EndTime, Objective, Reward, _participants, GuildId, CreatedByUserId, result);
    }

    /// <summary>
    /// Gets the current rank of a participant among active (non-withdrawn) participants.
    /// </summary>
    public int GetParticipantRank(Guid userId)
    {
        List<ChallengeParticipant> active = _participants
            .Where(p => !p.Withdrawn)
            .OrderByDescending(p => p.TasksCompleted)
            .ThenBy(p => p.LastCompletedAt)
            .ToList();

        for (int i = 0; i < active.Count; i++)
        {
            if (active[i].UserId == userId)
            {
                return i + 1;
            }
        }

        throw new Exceptions.DomainException("User is not an active participant in this challenge.");
    }

    /// <summary>
    /// Gets the top N participants for leaderboard display.
    /// </summary>
    public IReadOnlyList<ChallengeParticipant> GetTopParticipants(int count)
    {
        if (count < 1)
        {
            throw new Exceptions.DomainException("Count must be at least 1.");
        }

        return _participants
            .Where(p => !p.Withdrawn)
            .OrderByDescending(p => p.TasksCompleted)
            .ThenBy(p => p.LastCompletedAt)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Whether a specific user is participating (and has not withdrawn).
    /// </summary>
    public bool IsParticipating(Guid userId)
    {
        return _participants.Exists(p => p.UserId == userId && !p.Withdrawn);
    }
}
