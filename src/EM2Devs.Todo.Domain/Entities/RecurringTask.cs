using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

public sealed class RecurringTask
{
    public RecurringTaskId Id { get; }
    public TaskTitle Title { get; }
    public RecurrencePattern Pattern { get; }

    private RecurringTask(RecurringTaskId id, TaskTitle title, RecurrencePattern pattern)
    {
        Id = id;
        Title = title;
        Pattern = pattern;
    }

    public static RecurringTask Create(TaskTitle title, RecurrencePattern pattern)
    {
        return new RecurringTask(RecurringTaskId.New(), title, pattern);
    }

    public TodoTask GenerateNextInstance()
    {
        return TodoTask.Create(Title);
    }
}
