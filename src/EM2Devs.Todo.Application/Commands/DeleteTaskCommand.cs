using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record DeleteTaskCommand(Guid TaskId) : IRequest<bool>;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
{
    private readonly ITaskRepository _repository;

    public DeleteTaskCommandHandler(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _repository.DeleteAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);
    }
}
