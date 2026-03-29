using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class RecurringTask
{
    public RecurringTaskId Id { get; }
    public TaskTitle Title { get; }
    public RecurrencePattern Pattern { get; }
    public bool IsActive { get; private set; }

    private RecurringTask(RecurringTaskId id, TaskTitle title, RecurrencePattern pattern)
    {
        Id = id;
        Title = title;
        Pattern = pattern;
        IsActive = true;
    }

    public static RecurringTask Create(TaskTitle title, RecurrencePattern pattern)
    {
        return new RecurringTask(RecurringTaskId.New(), title, pattern);
    }

    public TodoTask GenerateNextInstance()
    {
        if (!IsActive)
        {
            throw new DomainException("Cannot generate instances for a paused recurring task.");
        }

        return TodoTask.Create(Title);
    }

    public void Pause()
    {
        if (!IsActive)
        {
            throw new DomainException("Recurring task is already paused.");
        }

        IsActive = false;
    }

    public void Resume()
    {
        if (IsActive)
        {
            throw new DomainException("Recurring task is already active.");
        }

        IsActive = true;
    }
}
