namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A guild quest with a title, description, due date, and assignable tasks.
/// Progress is derived from completed tasks.
/// </summary>
public sealed record GuildQuest
{
    private readonly List<GuildTask> _tasks;

    public GuildQuestId Id { get; }
    public string Title { get; }
    public string Description { get; }
    public DateOnly? DueDate { get; }
    public bool IsCompleted { get; }
    public IReadOnlyList<GuildTask> Tasks => _tasks.AsReadOnly();
    public int CompletedTaskCount => _tasks.Count(t => t.IsCompleted);
    public int TotalTaskCount => _tasks.Count;

    public GuildQuest(GuildQuestId id, string title, string description, DateOnly? dueDate,
        IEnumerable<GuildTask> tasks, bool isCompleted = false)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new Exceptions.DomainException("Guild quest title cannot be empty.");
        }

        if (title.Length > 100)
        {
            throw new Exceptions.DomainException("Guild quest title cannot exceed 100 characters.");
        }

        _tasks = tasks?.ToList() ?? throw new ArgumentNullException(nameof(tasks));

        Title = title;
        Description = description ?? string.Empty;
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    /// <summary>
    /// Complete a task within this quest by task ID. Returns the updated quest.
    /// If all tasks are now completed, the quest itself is marked as completed.
    /// </summary>
    public GuildQuest CompleteTask(GuildTaskId taskId)
    {
        ArgumentNullException.ThrowIfNull(taskId);

        GuildTask? task = _tasks.Find(t => t.Id == taskId);
        if (task is null)
        {
            throw new Exceptions.DomainException("Task not found in this guild quest.");
        }

        List<GuildTask> updatedTasks = _tasks
            .Select(t => t.Id == taskId ? t.Complete() : t)
            .ToList();

        bool allComplete = updatedTasks.All(t => t.IsCompleted);
        return new GuildQuest(Id, Title, Description, DueDate, updatedTasks, allComplete);
    }

    /// <summary>
    /// Unassign all tasks belonging to a specific user.
    /// </summary>
    public GuildQuest UnassignTasksForUser(Guid userId)
    {
        List<GuildTask> updatedTasks = _tasks
            .Select(t => t.AssigneeUserId == userId && !t.IsCompleted ? t.Unassign() : t)
            .ToList();

        return new GuildQuest(Id, Title, Description, DueDate, updatedTasks, IsCompleted);
    }

    /// <summary>
    /// Get the list of tasks assigned to a specific user.
    /// </summary>
    public IReadOnlyList<GuildTask> TasksForUser(Guid userId)
    {
        return _tasks.Where(t => t.AssigneeUserId == userId).ToList().AsReadOnly();
    }

    /// <summary>
    /// Returns the progress as a fraction (0.0 to 1.0). Zero if no tasks.
    /// </summary>
    public double Progress => TotalTaskCount == 0 ? 0.0 : (double)CompletedTaskCount / TotalTaskCount;
}
