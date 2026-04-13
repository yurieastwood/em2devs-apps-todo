using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class TodoTask
{
    public TaskId Id { get; }
    public TaskTitle Title { get; private set; }
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public bool IsBossTask { get; private set; }
    public TaskDifficulty Difficulty { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TimeEstimate? EstimatedTime { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public RecurringTaskId? SourceRecurringTaskId { get; private set; }
    public DateOnly? ScheduledDate { get; private set; }
    public int RescheduleCount { get; private set; }
    public int ViewCount { get; private set; }

    public bool IsOverdue => ScheduledDate.HasValue
        && ScheduledDate.Value < DateOnly.FromDateTime(DateTime.UtcNow)
        && Status != TaskStatus.Done
        && Status != TaskStatus.Skipped;

    public bool WasCompletedLate => ScheduledDate.HasValue
        && CompletedAt.HasValue
        && DateOnly.FromDateTime(CompletedAt.Value.UtcDateTime) > ScheduledDate.Value;

    private TodoTask(TaskId id, TaskTitle title, TaskDifficulty difficulty, DateTimeOffset? dueDate,
        TaskPriority priority = TaskPriority.Medium,
        RecurringTaskId? sourceRecurringTaskId = null, DateOnly? scheduledDate = null,
        DateTimeOffset? createdAt = null)
    {
        Id = id;
        Title = title;
        Status = TaskStatus.Todo;
        Difficulty = difficulty;
        Priority = priority;
        DueDate = dueDate;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        SourceRecurringTaskId = sourceRecurringTaskId;
        ScheduledDate = scheduledDate;
    }

    public static TodoTask Create(TaskTitle title, TaskDifficulty difficulty = TaskDifficulty.Normal,
        DateTimeOffset? dueDate = null, TaskPriority priority = TaskPriority.Medium,
        DateTimeOffset? createdAt = null)
    {
        return new TodoTask(TaskId.New(), title, difficulty, dueDate, priority, createdAt: createdAt);
    }

    public static TodoTask CreateFromRecurring(TaskTitle title, RecurringTaskId sourceId, DateOnly scheduledDate,
        TaskDifficulty difficulty = TaskDifficulty.Normal)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        var dueDate = scheduledDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return new TodoTask(TaskId.New(), title, difficulty, new DateTimeOffset(dueDate),
            sourceRecurringTaskId: sourceId, scheduledDate: scheduledDate);
    }

    public void MoveToInProgress()
    {
        if (Status != TaskStatus.Todo)
        {
            throw new DomainException($"Cannot transition from {Status} to {TaskStatus.InProgress}.");
        }

        Status = TaskStatus.InProgress;
    }

    public void MarkAsDone()
    {
        if (Status != TaskStatus.InProgress)
        {
            throw new DomainException($"Cannot transition from {Status} to {TaskStatus.Done}.");
        }

        Status = TaskStatus.Done;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Reopen()
    {
        if (Status != TaskStatus.Done)
        {
            throw new DomainException($"Cannot reopen a task that is not completed. Current status: {Status}.");
        }

        Status = TaskStatus.Todo;
        CompletedAt = null;
    }

    public void UpdateTitle(TaskTitle newTitle)
    {
        ArgumentNullException.ThrowIfNull(newTitle);
        Title = newTitle;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateDifficulty(TaskDifficulty difficulty)
    {
        Difficulty = difficulty;
    }

    public void UpdatePriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void UpdateEstimatedTime(TimeEstimate? estimatedTime)
    {
        EstimatedTime = estimatedTime;
    }

    public void UpdateDueDate(DateTimeOffset? dueDate)
    {
        DueDate = dueDate;
    }

    public void PromoteToBossTask()
    {
        if (Status == TaskStatus.Done)
        {
            throw new DomainException("Cannot promote a completed task to Boss Task.");
        }

        IsBossTask = true;
    }

    public void DemoteFromBossTask()
    {
        IsBossTask = false;
    }

    public void Reschedule()
    {
        if (Status == TaskStatus.Done)
        {
            throw new DomainException("Cannot reschedule a completed task.");
        }

        if (Status == TaskStatus.Skipped)
        {
            throw new DomainException("Cannot reschedule a skipped task.");
        }

        RescheduleCount++;
    }

    public void RecordView()
    {
        ViewCount++;
    }

    public void Skip()
    {
        if (Status == TaskStatus.Done)
        {
            throw new DomainException("Cannot skip a completed task.");
        }

        if (Status == TaskStatus.Skipped)
        {
            throw new DomainException("Task is already skipped.");
        }

        Status = TaskStatus.Skipped;
    }

    public void Delete()
    {
        if (Status == TaskStatus.Done)
        {
            throw new DomainException("Cannot delete a completed task.");
        }

        if (Status == TaskStatus.Deleted)
        {
            throw new DomainException("Task is already deleted.");
        }

        IsBossTask = false;
        Status = TaskStatus.Deleted;
    }
}
