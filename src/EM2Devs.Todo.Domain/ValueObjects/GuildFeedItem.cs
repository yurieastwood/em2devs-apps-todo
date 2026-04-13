namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Types of guild activity feed events.
/// </summary>
public enum GuildFeedEventType
{
    MemberJoined,
    MemberLeft,
    MemberRemoved,
    TaskCompleted,
    QuestCompleted,
    GuildLevelUp,
    LeadershipTransferred
}

/// <summary>
/// A single entry in the guild activity feed.
/// </summary>
public sealed record GuildFeedItem
{
    public GuildFeedEventType EventType { get; }
    public Guid UserId { get; }
    public string Description { get; }
    public DateTimeOffset OccurredAt { get; }

    public GuildFeedItem(GuildFeedEventType eventType, Guid userId, string description, DateTimeOffset occurredAt)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Feed item user ID cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new Exceptions.DomainException("Feed item description cannot be empty.");
        }

        EventType = eventType;
        UserId = userId;
        Description = description;
        OccurredAt = occurredAt;
    }
}
