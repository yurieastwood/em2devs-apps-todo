namespace EM2Devs.Todo.Application.ReadModels;

public sealed record CapacityOverviewReadModel(
    IReadOnlyDictionary<string, int> CapacityByDay,
    string MostProductiveDay,
    string LeastProductiveDay,
    int AverageDailyCapacity,
    int TodayCapacity,
    int TodayScheduled,
    bool IsOvercommitted,
    string? PlanningRecommendation);
