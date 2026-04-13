namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a suggested breakdown of a Boss Task into smaller subtasks (2-5).
/// Maps to: boss-tasks.feature — "Offer task breakdown when a Boss Task is encountered"
/// </summary>
public sealed record BossTaskBreakdown
{
    private const int MinSubtasks = 2;
    private const int MaxSubtasks = 5;

    public TaskId ParentTaskId { get; }
    public IReadOnlyList<TaskTitle> SuggestedSubtasks { get; }

    private BossTaskBreakdown(TaskId parentTaskId, IReadOnlyList<TaskTitle> suggestedSubtasks)
    {
        ParentTaskId = parentTaskId;
        SuggestedSubtasks = suggestedSubtasks;
    }

    public static BossTaskBreakdown Create(TaskId parentTaskId, IReadOnlyCollection<TaskTitle> subtaskTitles)
    {
        ArgumentNullException.ThrowIfNull(subtaskTitles);

        if (subtaskTitles.Count < MinSubtasks)
        {
            throw new Exceptions.DomainException(
                $"Boss Task breakdown requires at least {MinSubtasks} subtasks.");
        }

        if (subtaskTitles.Count > MaxSubtasks)
        {
            throw new Exceptions.DomainException(
                $"Boss Task breakdown allows at most {MaxSubtasks} subtasks.");
        }

        return new BossTaskBreakdown(parentTaskId, subtaskTitles.ToList().AsReadOnly());
    }

    /// <summary>
    /// Accepts the breakdown, creating TodoTask instances for each suggested subtask.
    /// </summary>
    public IReadOnlyList<Entities.TodoTask> Accept()
    {
        return SuggestedSubtasks
            .Select(title => Entities.TodoTask.Create(title))
            .ToList()
            .AsReadOnly();
    }
}
