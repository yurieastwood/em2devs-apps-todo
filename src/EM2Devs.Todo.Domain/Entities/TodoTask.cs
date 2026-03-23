using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class TodoTask
{
    public TaskId Id { get; }
    public TaskTitle Title { get; private set; }
    public TaskStatus Status { get; private set; }
    public bool IsBossTask { get; private set; }
    public TaskDifficulty Difficulty { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private TodoTask(TaskId id, TaskTitle title, TaskDifficulty difficulty, DateTimeOffset? dueDate)
    {
        Id = id;
        Title = title;
        Status = TaskStatus.Todo;
        Difficulty = difficulty;
        DueDate = dueDate;
    }

    public static TodoTask Create(TaskTitle title, TaskDifficulty difficulty = TaskDifficulty.Normal, DateTimeOffset? dueDate = null)
    {
        return new TodoTask(TaskId.New(), title, difficulty, dueDate);
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
}
