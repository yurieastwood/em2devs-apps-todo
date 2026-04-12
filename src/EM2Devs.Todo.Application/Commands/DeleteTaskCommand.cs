using EM2Devs.Todo.Application.Events;
using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record DeleteTaskCommand(Guid TaskId) : IRequest<Result<bool>>;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<bool>>
{
    private readonly ITaskRepository _repository;
    private readonly IMediator _mediator;

    public DeleteTaskCommandHandler(ITaskRepository repository, IMediator mediator)
    {
        _repository = repository;
        _mediator = mediator;
    }

    public async Task<Result<bool>> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        TaskId taskId = new(request.TaskId);

        bool deleted = await _repository.DeleteAsync(taskId, ct).ConfigureAwait(false);

        if (!deleted)
        {
            return new NotFoundError($"Task with id '{request.TaskId}' was not found.");
        }

        await _mediator.Publish(new TaskDeletedEvent(taskId), ct).ConfigureAwait(false);

        return true;
    }
}
