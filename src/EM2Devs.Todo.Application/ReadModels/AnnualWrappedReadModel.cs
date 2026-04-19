namespace EM2Devs.Todo.Application.ReadModels;

public sealed record AnnualWrappedReadModel(
    int Year,
    bool IsPartialYear,
    IReadOnlyList<WrappedSlideReadModel> Slides);

public sealed record WrappedSlideReadModel(
    string Title,
    string Metric,
    string VisualizationType,
    bool IsShareable);
