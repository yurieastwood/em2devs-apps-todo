using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record DeleteTaskCommand(Guid TaskId) : IRequest<Result<bool>>;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<bool>>
{
    private readonly ITaskRepository _repository;

    public DeleteTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool deleted = await _repository.DeleteAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);

        if (!deleted)
        {
            return new NotFoundError($"Task with id '{request.TaskId}' was not found.");
        }

        return true;
    }
}
