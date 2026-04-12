using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class RecurringTask
{
    public RecurringTaskId Id { get; }
    public TaskTitle Title { get; private set; }
    public RecurrencePattern Pattern { get; private set; }
    public bool IsActive { get; private set; }
    public DateOnly? EndDate { get; }

    private RecurringTask(RecurringTaskId id, TaskTitle title, RecurrencePattern pattern,
        DateOnly? endDate = null)
    {
        Id = id;
        Title = title;
        Pattern = pattern;
        IsActive = true;
        EndDate = endDate;
    }

    public static RecurringTask Create(TaskTitle title, RecurrencePattern pattern,
        DateOnly? endDate = null)
    {
        if (endDate.HasValue && endDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new DomainException("Cannot create a recurring task with an end date in the past.");
        }

        return new RecurringTask(RecurringTaskId.New(), title, pattern, endDate);
    }

    public TodoTask GenerateNextInstance(DateOnly scheduledDate)
    {
        if (!IsActive)
        {
            throw new DomainException("Cannot generate instances for a paused recurring task.");
        }

        if (EndDate.HasValue && scheduledDate > EndDate.Value)
        {
            throw new DomainException("Cannot generate instances after the end date.");
        }

        return TodoTask.CreateFromRecurring(Title, Id, scheduledDate);
    }

    /// <summary>
    /// Pure function: decides whether a new instance should be generated for <paramref name="today"/>
    /// given the scheduled date of the most recent instance (or <c>null</c> if none exists yet).
    ///
    /// The single source of truth for "last generation" is the instance table — this entity does not
    /// carry its own <c>LastGeneratedAt</c> field. The caller (typically <c>RecurringTaskGenerationJob</c>)
    /// queries the instances table for <c>MAX(scheduled_date)</c> scoped to this recurring task's Id
    /// and passes the result here.
    /// </summary>
    public bool IsDueForGeneration(DateOnly? lastScheduledDate, DateOnly today)
    {
        if (!IsActive)
        {
            return false;
        }

        if (EndDate.HasValue && today > EndDate.Value)
        {
            return false;
        }

        if (lastScheduledDate is null)
        {
            return true;
        }

        DateOnly last = lastScheduledDate.Value;

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
