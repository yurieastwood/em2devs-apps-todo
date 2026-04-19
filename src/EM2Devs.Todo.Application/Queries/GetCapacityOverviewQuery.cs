using EM2Devs.Todo.Application.Mediator;
using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Queries;

public sealed record GetCapacityOverviewQuery : IRequest<Result<CapacityOverviewReadModel>>;

public sealed class GetCapacityOverviewQueryHandler
    : IRequestHandler<GetCapacityOverviewQuery, Result<CapacityOverviewReadModel>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly TimeProvider _timeProvider;

    public GetCapacityOverviewQueryHandler(ITaskRepository taskRepository, TimeProvider timeProvider)
    {
        _taskRepository = taskRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CapacityOverviewReadModel>> Handle(GetCapacityOverviewQuery request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        IReadOnlyList<TodoTask> tasks = await _taskRepository.GetAllAsync(ct).ConfigureAwait(false);

        var completedByDay = tasks
            .Where(t => t.CompletedAt.HasValue && t.ScheduledDate.HasValue && t.ScheduledDate.Value < today)
            .GroupBy(t => t.ScheduledDate!.Value.DayOfWeek)
            .ToDictionary(g => g.Key, g =>
            {
                var dates = g.GroupBy(t => t.ScheduledDate!.Value).Select(d => d.Count()).ToList();
                return dates.Count > 0 ? (int)Math.Round(dates.Average()) : 0;
            });

        if (completedByDay.Count < 7)
        {
            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                completedByDay.TryAdd(day, 0);
            }
        }

        WeeklyCapacityOverview overview = WeeklyCapacityOverview.From(completedByDay);

        int todayCapacity = overview.CapacityByDay.TryGetValue(today.DayOfWeek, out int cap) ? cap : 0;
        int todayScheduled = tasks.Count(t =>
            t.ScheduledDate == today && t.CompletedAt is null);
        bool isOvercommitted = todayScheduled > todayCapacity && todayCapacity > 0;

        string? planningRecommendation = CapacityInsight.GetPlanningRecommendation(overview);

        return new CapacityOverviewReadModel(
            overview.CapacityByDay.ToDictionary(k => k.Key.ToString(), k => k.Value),
            overview.MostProductiveDay.ToString(),
            overview.LeastProductiveDay.ToString(),
            overview.AverageDailyCapacity,
            todayCapacity,
            todayScheduled,
            isOvercommitted,
            planningRecommendation);
    }
}
