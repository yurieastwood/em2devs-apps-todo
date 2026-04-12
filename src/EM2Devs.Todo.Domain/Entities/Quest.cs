using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class Quest
{
    private readonly List<TodoTask> _tasks = [];

    public QuestId Id { get; }
    public QuestTitle Title { get; private set; }
    public string Description { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public EpicId? EpicId { get; private set; }
    public ExperiencePoints TotalXpEarned { get; private set; }
    public IReadOnlyList<TodoTask> Tasks => _tasks.AsReadOnly();

    public int Progress
    {
        get
        {
            if (_tasks.Count == 0)
            {
                return 0;
            }

            int completed = _tasks.Count(t => t.Status == TaskStatus.Done);
            return completed * 100 / _tasks.Count;
        }
    }

    private Quest(QuestId id, QuestTitle title, string description, DateOnly? dueDate)
    {
        Id = id;
        Title = title;
        Description = description;
        DueDate = dueDate;
        TotalXpEarned = new ExperiencePoints(0);
    }

    public static Quest Create(QuestTitle title, string description, DateOnly? dueDate = null)
    {
        return new Quest(QuestId.New(), title, description, dueDate);
    }

    public void AddTask(TodoTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (_tasks.Any(t => t.Id == task.Id))
        {
            throw new DomainException("Task is already assigned to this quest.");
        }

        _tasks.Add(task);
    }

    public void ReplaceTask(TodoTask updatedTask)
    {
        ArgumentNullException.ThrowIfNull(updatedTask);

        int index = _tasks.FindIndex(t => t.Id == updatedTask.Id);
        if (index < 0)
        {
            throw new DomainException($"Task with id '{updatedTask.Id.Value}' is not assigned to this quest.");
        }

        _tasks[index] = updatedTask;
    }

    public void RemoveTask(TaskId taskId)
    {
        ArgumentNullException.ThrowIfNull(taskId);

        TodoTask? task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
        {
            throw new DomainException($"Task with id '{taskId.Value}' is not assigned to this quest.");
        }

        _tasks.Remove(task);
    }

    public void AssignToEpic(EpicId epicId)
    {
        ArgumentNullException.ThrowIfNull(epicId);

        if (EpicId is not null)
        {
            throw new DomainException("Quest already belongs to an epic. Remove it from the current epic first, or move it.");
        }

        EpicId = epicId;
    }

    public void UnassignFromEpic()
    {
        if (EpicId is null)
        {
            throw new DomainException("Quest is not assigned to any epic.");
        }

        EpicId = null;
    }

    public void AddXpEarned(ExperiencePoints xp)
    {
        ArgumentNullException.ThrowIfNull(xp);
        TotalXpEarned = TotalXpEarned.Add(xp);
    }

    public void Complete()
    {
        if (IsCompleted)
        {
            throw new DomainException("Quest is already completed.");
        }

        if (_tasks.Count == 0)
        {
            throw new DomainException("Cannot complete a quest with no tasks.");
        }

        if (Progress < 100)
        {
            throw new DomainException("Cannot complete a quest when not all tasks are done.");
        }

        IsCompleted = true;
    }
}
