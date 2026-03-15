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
    }

    public static Quest Create(QuestTitle title, string description, DateOnly? dueDate)
    {
        return new Quest(QuestId.New(), title, description, dueDate);
    }

    public void AddTask(TodoTask task)
    {
        if (_tasks.Any(t => t.Id == task.Id))
        {
            throw new DomainException("Task is already assigned to this quest.");
        }

        _tasks.Add(task);
    }
}
