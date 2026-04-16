using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Commands;

public sealed record StartFocusModeCommand(Guid TaskId) : IRequest<Result<bool>>;

public sealed class StartFocusModeCommandHandler
    : IRequestHandler<StartFocusModeCommand, Result<bool>>
{
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly TimeProvider _timeProvider;

    public StartFocusModeCommandHandler(
        IPlayerProfileRepository profileRepository,
        ITaskRepository taskRepository,
        TimeProvider timeProvider)
    {
        _profileRepository = profileRepository;
        _taskRepository = taskRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<bool>> Handle(StartFocusModeCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        Domain.Entities.TodoTask? task = await _taskRepository
            .GetByIdAsync(new TaskId(request.TaskId), ct).ConfigureAwait(false);

        if (task is null)
        {
            return new NotFoundError("Task not found.");
        }

        if (!task.IsBossTask)
        {
            return new ValidationError("Focus mode is only available for Boss Tasks.");
        }

        try
        {
            await _profileRepository.StartFocusModeAsync(
                new TaskId(request.TaskId), _timeProvider.GetUtcNow(), ct).ConfigureAwait(false);

            return true;
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return new ConflictError(ex.Message);
        }
    }
}

public sealed record EndFocusModeCommand : IRequest<Result<FocusModeResult>>;

public sealed record FocusModeResult(Guid TaskId, int DurationMinutes);

public sealed class EndFocusModeCommandHandler
    : IRequestHandler<EndFocusModeCommand, Result<FocusModeResult>>
{
    private readonly IPlayerProfileRepository _profileRepository;
    private readonly TimeProvider _timeProvider;

    public EndFocusModeCommandHandler(
        IPlayerProfileRepository profileRepository,
        TimeProvider timeProvider)
    {
        _profileRepository = profileRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<FocusModeResult>> Handle(EndFocusModeCommand request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            FocusMode ended = await _profileRepository
                .EndFocusModeAsync(_timeProvider.GetUtcNow(), ct).ConfigureAwait(false);

            return new FocusModeResult(ended.TaskId.Value, (int)ended.Duration.TotalMinutes);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            return new ConflictError(ex.Message);
        }
    }
}
