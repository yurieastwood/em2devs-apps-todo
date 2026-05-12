using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class RecurringTask
{
    public RecurringTaskId Id { get; }
    public Guid UserId { get; private set; }
    public TaskTitle Title { get; private set; }
    public RecurrencePattern Pattern { get; private set; }
    public bool IsActive { get; private set; }
    public DateOnly? EndDate { get; }

    private RecurringTask(RecurringTaskId id, Guid userId, TaskTitle title, RecurrencePattern pattern,
        DateOnly? endDate = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        Id = id;
        UserId = userId;
        Title = title;
        Pattern = pattern;
        IsActive = true;
        EndDate = endDate;
    }

    // Stryker disable all : EF Core materialization constructor — not reachable from domain tests.
    // EF binds these parameters to mapped properties by name+type. Parameter types must exactly
    // match the mapped property types.
    private RecurringTask(RecurringTaskId id, Guid userId, TaskTitle title, RecurrencePattern pattern,
        bool isActive, DateOnly? endDate)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Pattern = pattern;
        IsActive = isActive;
        EndDate = endDate;
    }
    // Stryker restore all

    public static RecurringTask Create(Guid userId, TaskTitle title, RecurrencePattern pattern,
        DateOnly? endDate = null)
    {
        if (endDate.HasValue && endDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new DomainException("Cannot create a recurring task with an end date in the past.");
        }

        return new RecurringTask(RecurringTaskId.New(), userId, title, pattern, endDate);
    }

    /// <summary>
    /// Rebuilds a recurring task from a persisted snapshot. No validation of the end-date
    /// past-check since the snapshot is trusted (existing user data being restored).
    /// </summary>
    public static RecurringTask Reconstitute(
        RecurringTaskId id,
        Guid userId,
        TaskTitle title,
        RecurrencePattern pattern,
        bool isActive,
        DateOnly? endDate)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(title);
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        return new RecurringTask(id, userId, title, pattern, isActive, endDate);
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

        return TodoTask.CreateFromRecurring(UserId, Title, Id, scheduledDate);
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
