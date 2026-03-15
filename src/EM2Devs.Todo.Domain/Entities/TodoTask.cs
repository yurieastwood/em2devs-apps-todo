using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class TodoTask
{
    public TaskId Id { get; }
    public TaskTitle Title { get; private set; }
    public TaskStatus Status { get; private set; }
    public bool IsBossTask { get; private set; }

    private TodoTask(TaskId id, TaskTitle title)
    {
        Id = id;
        Title = title;
        Status = TaskStatus.Todo;
    }

    public static TodoTask Create(TaskTitle title)
    {
        return new TodoTask(TaskId.New(), title);
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
