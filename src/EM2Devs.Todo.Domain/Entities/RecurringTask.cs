using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class RecurringTask
{
    public RecurringTaskId Id { get; }
    public TaskTitle Title { get; private set; }
    public RecurrencePattern Pattern { get; private set; }
    public bool IsActive { get; private set; }
    public DateOnly? LastGeneratedAt { get; private set; }

    private RecurringTask(RecurringTaskId id, TaskTitle title, RecurrencePattern pattern)
    {
        Id = id;
        Title = title;
        Pattern = pattern;
        IsActive = true;
        LastGeneratedAt = null;
    }

    public static RecurringTask Create(TaskTitle title, RecurrencePattern pattern)
    {
        return new RecurringTask(RecurringTaskId.New(), title, pattern);
    }

    public TodoTask GenerateNextInstance(DateOnly scheduledDate)
    {
        if (!IsActive)
        {
            throw new DomainException("Cannot generate instances for a paused recurring task.");
        }

        return TodoTask.CreateFromRecurring(Title, Id, scheduledDate);
    }

    public void MarkInstanceGenerated(DateOnly generatedDate)
    {
        LastGeneratedAt = generatedDate;
    }

    public bool IsDueForGeneration(DateOnly today)
    {
        if (!IsActive)
        {
            return false;
        }

        if (LastGeneratedAt is null)
        {
            return true;
        }

        DateOnly last = LastGeneratedAt.Value;

        return Pattern switch
        {
            RecurrencePattern.Daily => last < today,
            RecurrencePattern.Weekly => today.DayNumber - last.DayNumber >= 7,
            RecurrencePattern.Monthly => last.Year != today.Year || last.Month != today.Month,
            _ => false
        };
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

    public void UpdateTitle(TaskTitle newTitle)
    {
        ArgumentNullException.ThrowIfNull(newTitle);
        Title = newTitle;
    }

    public void UpdatePattern(RecurrencePattern newPattern)
    {
        if (!Enum.IsDefined(newPattern))
        {
            throw new DomainException($"Invalid recurrence pattern: '{newPattern}'.");
        }

        Pattern = newPattern;
    }
}
