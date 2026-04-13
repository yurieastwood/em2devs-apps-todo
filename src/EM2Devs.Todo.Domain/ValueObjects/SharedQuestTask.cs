namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed shared quest identifier (ADR-0023).
/// </summary>
public sealed record SharedQuestId(Guid Value)
{
    public static SharedQuestId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed shared quest task identifier (ADR-0023).
/// </summary>
public sealed record SharedQuestTaskId(Guid Value)
{
    public static SharedQuestTaskId New() => new(Guid.NewGuid());
}

/// <summary>
/// A task within a shared quest, with a title, optional assignee, and completion status.
/// </summary>
public sealed record SharedQuestTask
{
    public SharedQuestTaskId Id { get; }
    public string Title { get; }
    public Guid? AssigneeUserId { get; }
    public bool IsCompleted { get; }

    public SharedQuestTask(SharedQuestTaskId id, string title, Guid? assigneeUserId = null, bool isCompleted = false)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new Exceptions.DomainException("Shared quest task title cannot be empty.");
        }

        if (title.Length > 200)
        {
            throw new Exceptions.DomainException("Shared quest task title cannot exceed 200 characters.");
        }

        Title = title;
        AssigneeUserId = assigneeUserId;
        IsCompleted = isCompleted;
    }

    /// <summary>
    /// Mark this task as completed.
    /// </summary>
    public SharedQuestTask Complete()
    {
        if (IsCompleted)
        {
            throw new Exceptions.DomainException("Shared quest task is already completed.");
        }

        return new SharedQuestTask(Id, Title, AssigneeUserId, isCompleted: true);
    }

    /// <summary>
    /// Remove the assignee from this task (make it unassigned).
    /// </summary>
    public SharedQuestTask Unassign()
    {
        return new SharedQuestTask(Id, Title, assigneeUserId: null, IsCompleted);
    }

    /// <summary>
    /// Assign a user to this task.
    /// </summary>
    public SharedQuestTask AssignTo(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new Exceptions.DomainException("Cannot assign task to an empty user ID.");
        }

        return new SharedQuestTask(Id, Title, userId, IsCompleted);
    }
}
