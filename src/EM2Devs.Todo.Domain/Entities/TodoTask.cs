using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class TodoTask
{
    public TaskId Id { get; }
    public Guid UserId { get; private set; }
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
    public string? WaitingReason { get; private set; }
    public CommitmentNote? CommitmentNote { get; private set; }
    public QuestId? AssignedQuestId { get; private set; }
    public EstimationRecord? ActualTimeRecord { get; private set; }

    private readonly List<ProcrastinationSignal> _procrastinationSignals = [];
    public IReadOnlyList<ProcrastinationSignal> ProcrastinationSignals => _procrastinationSignals.AsReadOnly();

    private readonly List<Tag> _tags = [];
    /// <summary>Tags attached to this task. Duplicates (after normalisation) are ignored.</summary>
    public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();

    public bool IsOverdue => ScheduledDate.HasValue
        && ScheduledDate.Value < DateOnly.FromDateTime(DateTime.UtcNow)
        && Status != TaskStatus.Done
        && Status != TaskStatus.Skipped;

    public bool WasCompletedLate => ScheduledDate.HasValue
        && CompletedAt.HasValue
        && DateOnly.FromDateTime(CompletedAt.Value.UtcDateTime) > ScheduledDate.Value;

    private TodoTask(TaskId id, Guid userId, TaskTitle title, TaskDifficulty difficulty, DateTimeOffset? dueDate,
        TaskPriority priority = TaskPriority.Medium,
        RecurringTaskId? sourceRecurringTaskId = null, DateOnly? scheduledDate = null,
        DateTimeOffset? createdAt = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        Id = id;
        UserId = userId;
        Title = title;
        Status = TaskStatus.Todo;
        Difficulty = difficulty;
        Priority = priority;
        DueDate = dueDate;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        SourceRecurringTaskId = sourceRecurringTaskId;
        ScheduledDate = scheduledDate;
    }

    // Stryker disable all : EF Core materialization constructor — not reachable from domain tests.
    // EF binds these parameters to mapped properties by name+type. Parameter types must exactly
    // match the mapped property types (notably non-nullable CreatedAt/Difficulty/Priority).
    private TodoTask(TaskId id, Guid userId, TaskTitle title, TaskDifficulty difficulty, TaskPriority priority,
        DateTimeOffset createdAt, DateTimeOffset? dueDate,
        RecurringTaskId? sourceRecurringTaskId, DateOnly? scheduledDate)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Difficulty = difficulty;
        Priority = priority;
        CreatedAt = createdAt;
        DueDate = dueDate;
        SourceRecurringTaskId = sourceRecurringTaskId;
        ScheduledDate = scheduledDate;
        // Status, IsBossTask, RescheduleCount, ViewCount and the rest are EF-set
        // through their private setters after this constructor returns.
    }
    // Stryker restore all

    public static TodoTask Create(Guid userId, TaskTitle title, TaskDifficulty difficulty = TaskDifficulty.Normal,
        DateTimeOffset? dueDate = null, TaskPriority priority = TaskPriority.Medium,
        DateTimeOffset? createdAt = null, DateOnly? scheduledDate = null)
    {
        return new TodoTask(TaskId.New(), userId, title, difficulty, dueDate, priority,
            createdAt: createdAt, scheduledDate: scheduledDate);
    }

    public static TodoTask CreateFromRecurring(Guid userId, TaskTitle title, RecurringTaskId sourceId, DateOnly scheduledDate,
        TaskDifficulty difficulty = TaskDifficulty.Normal)
    {
        ArgumentNullException.ThrowIfNull(sourceId);
        var dueDate = scheduledDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        return new TodoTask(TaskId.New(), userId, title, difficulty, new DateTimeOffset(dueDate),
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

    public void SetWaitingReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("Waiting reason cannot be empty.");
        }

        WaitingReason = reason;
    }

    public void ClearWaitingReason()
    {
        WaitingReason = null;
    }

    public void AddProcrastinationSignal(ProcrastinationSignal signal)
    {
        ArgumentNullException.ThrowIfNull(signal);
        _procrastinationSignals.Add(signal);
    }

    public void ClearProcrastinationSignals()
    {
        _procrastinationSignals.Clear();
    }

    public void RescheduleWithCommitment(CommitmentNote note)
    {
        ArgumentNullException.ThrowIfNull(note);

        Reschedule();
        CommitmentNote = note;
    }

    /// <summary>Adds a tag. If an equivalent tag is already present it is a no-op.</summary>
    public void AddTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        if (!_tags.Contains(tag))
        {
            _tags.Add(tag);
        }
    }

    /// <summary>Removes a tag. If it is not present it is a no-op.</summary>
    public void RemoveTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        _tags.Remove(tag);
    }

    /// <summary>Returns true when the task has a tag equal to the supplied tag.</summary>
    public bool HasTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);
        return _tags.Contains(tag);
    }

    /// <summary>
    /// Returns true when either the title or the description contains the given keyword
    /// (case-insensitive). Null/empty keywords match nothing.
    /// </summary>
    public bool MatchesKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return false;
        }

        string needle = keyword.Trim();
        if (Title.Value.Contains(needle, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return Description is not null
            && Description.Contains(needle, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Assigns this task to a quest. Passing null unassigns.</summary>
    public void AssignToQuest(QuestId? questId)
    {
        AssignedQuestId = questId;
    }

    /// <summary>
    /// Records the actual time spent on this completed task and returns the
    /// resulting <see cref="EstimationRecord"/>, which is also stored on the task.
    /// Requires both an estimated time and a Done status.
    /// </summary>
    public EstimationRecord RecordActualTime(TimeEstimate actual)
    {
        ArgumentNullException.ThrowIfNull(actual);

        if (Status != TaskStatus.Done)
        {
            throw new DomainException("Actual time can only be recorded for completed tasks.");
        }

        if (EstimatedTime is null)
        {
            throw new DomainException("Actual time can only be recorded when an estimate exists.");
        }

        ActualTimeRecord = EstimationRecord.Create(EstimatedTime, actual);
        return ActualTimeRecord;
    }
}
