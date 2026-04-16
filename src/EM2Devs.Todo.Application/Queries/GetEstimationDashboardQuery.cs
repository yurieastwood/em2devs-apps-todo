using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetEstimationDashboardQuery : IRequest<Result<EstimationDashboardReadModel>>;

public sealed class GetEstimationDashboardQueryHandler
    : IRequestHandler<GetEstimationDashboardQuery, Result<EstimationDashboardReadModel>>
{
    private readonly ITaskRepository _taskRepository;

    public GetEstimationDashboardQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<EstimationDashboardReadModel>> Handle(GetEstimationDashboardQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);

        List<EstimationRecord> records = tasks
            .Where(t => t.CompletedAt.HasValue && t.ActualTimeRecord is not null)
            .Select(t => t.ActualTimeRecord!)
            .ToList();

        if (records.Count == 0)
        {
            return new EstimationDashboardReadModel(null, [], []);
        }

        EstimationDashboard dashboard = EstimationDashboard.Build(records, []);

        var perCategory = dashboard.PerCategory
            .Select(c => new CategoryAccuracyReadModel(c.Category.Value, c.RecordCount, c.AverageVariancePercent, c.AccuracyPercent))
            .ToList();

        return new EstimationDashboardReadModel(
            dashboard.OverallAccuracyPercent,
            perCategory,
            dashboard.AccuracyTrend.ToList());
    }
}
