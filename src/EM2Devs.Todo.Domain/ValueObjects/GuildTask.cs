namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A task within a guild quest, with a title, optional assignee, and completion status.
/// </summary>
public sealed record GuildTask
{
    public GuildTaskId Id { get; }
    public string Title { get; }
    public Guid? AssigneeUserId { get; }
    public bool IsCompleted { get; }

    public GuildTask(GuildTaskId id, string title, Guid? assigneeUserId = null, bool isCompleted = false)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new Exceptions.DomainException("Guild task title cannot be empty.");
        }

        if (title.Length > 200)
        {
            throw new Exceptions.DomainException("Guild task title cannot exceed 200 characters.");
        }

        Title = title;
        AssigneeUserId = assigneeUserId;
        IsCompleted = isCompleted;
    }

    /// <summary>
    /// Mark this task as completed.
    /// </summary>
    public GuildTask Complete()
    {
        if (IsCompleted)
        {
            throw new Exceptions.DomainException("Guild task is already completed.");
        }

        return new GuildTask(Id, Title, AssigneeUserId, isCompleted: true);
    }

    /// <summary>
    /// Remove the assignee from this task (make it unassigned).
    /// </summary>
    public GuildTask Unassign()
    {
        return new GuildTask(Id, Title, assigneeUserId: null, IsCompleted);
    }

    /// <summary>
    /// Assign a user to this task.
    /// </summary>
    public GuildTask AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Cannot assign task to an empty user ID.");
        }

        return new GuildTask(Id, Title, userId, IsCompleted);
    }
}
