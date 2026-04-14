namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Breadcrumb path of a task within the Saga &gt; Epic &gt; Quest &gt; Task hierarchy.
/// Any level higher than the task itself can be absent if the task is not nested under it.
/// </summary>
public sealed record TaskHierarchyPath
{
    public SagaId? SagaId { get; }
    public SagaTitle? SagaTitle { get; }
    public EpicId? EpicId { get; }
    public EpicTitle? EpicTitle { get; }
    public QuestId? QuestId { get; }
    public QuestTitle? QuestTitle { get; }
    public TaskId TaskId { get; }
    public TaskTitle TaskTitle { get; }

    public TaskHierarchyPath(
        TaskId taskId,
        TaskTitle taskTitle,
        QuestId? questId = null,
        QuestTitle? questTitle = null,
        EpicId? epicId = null,
        EpicTitle? epicTitle = null,
        SagaId? sagaId = null,
        SagaTitle? sagaTitle = null)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(taskTitle);

        if (questId is null && questTitle is not null)
        {
            throw new Exceptions.DomainException("Quest title requires a quest id.");
        }

        if (questTitle is null && questId is not null)
        {
            throw new Exceptions.DomainException("Quest id requires a quest title.");
        }

        if (epicId is null && epicTitle is not null)
        {
            throw new Exceptions.DomainException("Epic title requires an epic id.");
        }

        if (epicTitle is null && epicId is not null)
        {
            throw new Exceptions.DomainException("Epic id requires an epic title.");
        }

        if (sagaId is null && sagaTitle is not null)
        {
            throw new Exceptions.DomainException("Saga title requires a saga id.");
        }

        if (sagaTitle is null && sagaId is not null)
        {
            throw new Exceptions.DomainException("Saga id requires a saga title.");
        }

        if (epicId is not null && questId is null)
        {
            throw new Exceptions.DomainException("Task cannot reference an epic without a quest.");
        }

        if (sagaId is not null && epicId is null)
        {
            throw new Exceptions.DomainException("Task cannot reference a saga without an epic.");
        }

        TaskId = taskId;
        TaskTitle = taskTitle;
        QuestId = questId;
        QuestTitle = questTitle;
        EpicId = epicId;
        EpicTitle = epicTitle;
        SagaId = sagaId;
        SagaTitle = sagaTitle;
    }

    /// <summary>
    /// Renders a human-readable breadcrumb such as "Saga &gt; Epic &gt; Quest &gt; Task".
    /// Omits missing levels.
    /// </summary>
    public string Breadcrumb
    {
        get
        {
            List<string> parts = [];
            if (SagaTitle is not null)
            {
                parts.Add(SagaTitle.Value);
            }

            if (EpicTitle is not null)
            {
                parts.Add(EpicTitle.Value);
            }

            if (QuestTitle is not null)
            {
                parts.Add(QuestTitle.Value);
            }

            parts.Add(TaskTitle.Value);
            return string.Join(" > ", parts);
        }
    }
}
