using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetRecurringTaskQuery(Guid RecurringTaskId) : IRequest<Result<RecurringTask>>;

public sealed class GetRecurringTaskQueryHandler
    : IRequestHandler<GetRecurringTaskQuery, Result<RecurringTask>>
{
    private readonly IRecurringTaskRepository _repository;

    public GetRecurringTaskQueryHandler(IRecurringTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<RecurringTask>> Handle(GetRecurringTaskQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        RecurringTask? recurring = await _repository
            .GetByIdAsync(new RecurringTaskId(request.RecurringTaskId), ct).ConfigureAwait(false);
        if (recurring is null)
        {
            return new NotFoundError($"Recurring task with id '{request.RecurringTaskId}' was not found.");
        }

        return recurring;
    }
}

public sealed record ListRecurringTasksQuery : IRequest<Result<IReadOnlyList<RecurringTask>>>;

public sealed class ListRecurringTasksQueryHandler
    : IRequestHandler<ListRecurringTasksQuery, Result<IReadOnlyList<RecurringTask>>>
{
    private readonly IRecurringTaskRepository _repository;

    public ListRecurringTasksQueryHandler(IRecurringTaskRepository repository) =>
        _repository = repository;

    public async Task<Result<IReadOnlyList<RecurringTask>>> Handle(ListRecurringTasksQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<RecurringTask> tasks = await _repository.GetAllAsync(ct).ConfigureAwait(false);
        return Result<IReadOnlyList<RecurringTask>>.Success(tasks);
    }
}
