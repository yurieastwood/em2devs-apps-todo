using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record ListTasksQuery(string? StatusFilter, string? View = null) : IRequest<Result<IReadOnlyList<TodoTask>>>;

public sealed class ListTasksQueryHandler : IRequestHandler<ListTasksQuery, Result<IReadOnlyList<TodoTask>>>
{
    private readonly ITaskRepository _repository;
    private readonly TimeProvider _timeProvider;

    public ListTasksQueryHandler(ITaskRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<IReadOnlyList<TodoTask>>> Handle(ListTasksQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.View is not null && request.StatusFilter is not null)
        {
            return new ValidationError("The 'view' and 'status' query parameters are mutually exclusive.");
        }

        IReadOnlyList<TodoTask> tasks = await _repository.GetAllAsync(ct).ConfigureAwait(false);

        if (request.View is not null)
        {
            if (!Enum.TryParse<TaskView>(request.View, ignoreCase: true, out TaskView view))
            {
                return new ValidationError($"Invalid view '{request.View}'. Valid values: inbox, today, upcoming, completed.");
            }

            DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
            tasks = view switch
            {
                TaskView.Inbox => TaskViewFilter.ForInbox(tasks),
                TaskView.Today => TaskViewFilter.ForToday(tasks, today),
                TaskView.Upcoming => TaskViewFilter.ForUpcoming(tasks, today).SelectMany(g => g.Tasks).ToList(),
                TaskView.Completed => TaskViewFilter.ForCompleted(tasks).SelectMany(g => g.Tasks).ToList(),
                _ => tasks,
            };
            return Result<IReadOnlyList<TodoTask>>.Success(tasks);
        }

        if (request.StatusFilter is not null &&
            Enum.TryParse<Domain.TaskStatus>(request.StatusFilter, ignoreCase: false, out Domain.TaskStatus parsed))
        {
            tasks = tasks.Where(t => t.Status == parsed).ToList().AsReadOnly();
        }

        return Result<IReadOnlyList<TodoTask>>.Success(tasks);
    }
}
