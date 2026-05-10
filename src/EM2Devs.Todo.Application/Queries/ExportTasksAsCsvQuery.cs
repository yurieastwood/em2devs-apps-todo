using System.Globalization;
using System.Text;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record ExportTasksAsCsvQuery() : IRequest<Result<string>>;

public sealed class ExportTasksAsCsvQueryHandler
    : IRequestHandler<ExportTasksAsCsvQuery, Result<string>>
{
    internal const string Header =
        "id,title,description,status,difficulty,priority,baseXp,tags,dueDate,scheduledDate,completedAt,createdAt,assignedQuestId";

    private readonly ITaskRepository _taskRepository;

    public ExportTasksAsCsvQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<string>> Handle(ExportTasksAsCsvQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);

        StringBuilder sb = new();
        sb.Append(Header).Append("\r\n");

        foreach (TodoTask task in tasks)
        {
            int baseXp = ExperiencePoints.BaseForDifficulty(task.Difficulty).Value;
            string tags = string.Join(';', task.Tags.Select(t => t.Value));

            AppendRow(sb,
                task.Id.Value.ToString(),
                task.Title.Value,
                task.Description,
                task.Status.ToString(),
                task.Difficulty.ToString(),
                task.Priority.ToString(),
                baseXp.ToString(CultureInfo.InvariantCulture),
                tags,
                task.DueDate?.ToString("O", CultureInfo.InvariantCulture),
                task.ScheduledDate?.ToString("O", CultureInfo.InvariantCulture),
                task.CompletedAt?.ToString("O", CultureInfo.InvariantCulture),
                task.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
                task.AssignedQuestId?.Value.ToString());
        }

        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, params string?[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            sb.Append(EscapeField(fields[i]));
        }
        sb.Append("\r\n");
    }

    internal static string EscapeField(string? value)
    {
        if (value is null)
        {
            return string.Empty;
        }
        bool needsQuoting = value.IndexOfAny(['"', ',', '\r', '\n']) >= 0;
        if (!needsQuoting)
        {
            return value;
        }
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
