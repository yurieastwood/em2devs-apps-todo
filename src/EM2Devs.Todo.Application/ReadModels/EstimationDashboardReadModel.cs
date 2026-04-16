namespace EM2Devs.Todo.Application.ReadModels;

public sealed record EstimationDashboardReadModel(
    double? OverallAccuracyPercent,
    IReadOnlyList<CategoryAccuracyReadModel> PerCategory,
    IReadOnlyList<double> AccuracyTrend,
    string? ImprovementMessage = null);

public sealed record CategoryAccuracyReadModel(
    string Category,
    int RecordCount,
    double AverageVariancePercent,
    double AccuracyPercent);
