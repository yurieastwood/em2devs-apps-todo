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

        List<TodoTask> completedWithEstimates = tasks
            .Where(t => t.CompletedAt.HasValue && t.ActualTimeRecord is not null)
            .OrderBy(t => t.CompletedAt)
            .ToList();

        List<EstimationRecord> records = completedWithEstimates
            .Select(t => t.ActualTimeRecord!)
            .ToList();

        if (records.Count == 0)
        {
            return new EstimationDashboardReadModel(null, [], []);
        }

        List<double> trend = ComputeTrend(completedWithEstimates);
        EstimationDashboard dashboard = EstimationDashboard.Build(records, trend);

        var perCategory = dashboard.PerCategory
            .Select(c => new CategoryAccuracyReadModel(c.Category.Value, c.RecordCount, c.AverageVariancePercent, c.AccuracyPercent))
            .ToList();

        string? improvementMessage = DetectImprovement(completedWithEstimates);

        return new EstimationDashboardReadModel(
            dashboard.OverallAccuracyPercent,
            perCategory,
            dashboard.AccuracyTrend.ToList(),
            improvementMessage);
    }

    private static List<double> ComputeTrend(List<TodoTask> orderedTasks)
    {
        if (orderedTasks.Count < 4)
        {
            return [];
        }

        int chunkSize = Math.Max(1, orderedTasks.Count / 4);
        List<double> trend = [];
        for (int i = 0; i < orderedTasks.Count; i += chunkSize)
        {
            var chunk = orderedTasks.Skip(i).Take(chunkSize).ToList();
            double avgVariance = chunk.Average(t => Math.Abs(t.ActualTimeRecord!.VariancePercent));
            trend.Add(Math.Round(Math.Max(0.0, 100.0 - avgVariance), 2));
        }

        return trend;
    }

    private static string? DetectImprovement(List<TodoTask> orderedTasks)
    {
        if (orderedTasks.Count < 6)
        {
            return null;
        }

        int half = orderedTasks.Count / 2;
        double earlyVariance = orderedTasks.Take(half).Average(t => Math.Abs(t.ActualTimeRecord!.VariancePercent));
        double recentVariance = orderedTasks.Skip(half).Average(t => Math.Abs(t.ActualTimeRecord!.VariancePercent));

        if (recentVariance < earlyVariance)
        {
            double improvement = Math.Round(earlyVariance - recentVariance, 1);
            return $"Your estimation accuracy has improved by {improvement}% — keep it up!";
        }

        return null;
    }
}
