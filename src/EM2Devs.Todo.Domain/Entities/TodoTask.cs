using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class TodoTask
{
    public TaskId Id { get; }
    public TaskTitle Title { get; private set; }
    public TaskStatus Status { get; private set; }

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

    // TODO: Agent implements status transitions here (see ADR-0003 test scenarios)
}
