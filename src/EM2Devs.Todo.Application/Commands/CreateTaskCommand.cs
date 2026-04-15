using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record CreateTaskCommand(string Title) : IRequest<Result<TodoTask>>;

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

        TodoTask task = TodoTask.Create(_currentUser.UserId, title);
        await _repository.SaveAsync(task, ct).ConfigureAwait(false);
        return task;
    }
}
