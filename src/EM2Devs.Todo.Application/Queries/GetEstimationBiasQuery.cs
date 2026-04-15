using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

/// <summary>
/// Returns the authenticated user's estimation calibration — the median bias factor
/// (actual / estimated) across all completed tasks where an actual time was reported,
/// plus the sample size that produced it. Stateless: the calibration is recomputed
/// from the current task history on each call.
/// </summary>
public sealed record GetEstimationBiasQuery : IRequest<Result<EstimationCalibrationReadModel>>;

public sealed class GetEstimationBiasQueryHandler
    : IRequestHandler<GetEstimationBiasQuery, Result<EstimationCalibrationReadModel>>
{
    private readonly ITaskRepository _taskRepository;

    public GetEstimationBiasQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<EstimationCalibrationReadModel>> Handle(GetEstimationBiasQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);
        EstimationCalibration calibration = TimeEstimationCalibrator.Calibrate(tasks);

        return new EstimationCalibrationReadModel(
            calibration.BiasFactor,
            calibration.SampleSize,
            calibration.State.ToString());
    }
}
