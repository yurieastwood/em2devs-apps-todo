using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record CreateTaskCommand(
    string Title,
    DateOnly? ScheduledDate = null,
    IReadOnlyList<string>? Tags = null) : IRequest<Result<TodoTask>>;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TodoTask>>
{
    private readonly ITaskRepository _repository;
    private readonly ICurrentUser _currentUser;

    public CreateTaskCommandHandler(ITaskRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Result<TodoTask>> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TaskTitle title;
        try
        {
            title = new TaskTitle(request.Title);
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        TodoTask task;
        try
        {
            task = TodoTask.Create(_currentUser.UserId, title, scheduledDate: request.ScheduledDate);

            if (request.Tags is not null)
            {
                foreach (string rawTag in request.Tags)
                {
                    task.AddTag(Tag.From(rawTag));
                }
            }
        }
        catch (DomainException ex)
        {
            return new ValidationError(ex.Message);
        }

        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
