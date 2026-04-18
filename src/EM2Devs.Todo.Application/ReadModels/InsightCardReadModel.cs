namespace EM2Devs.Todo.Application.ReadModels;

public sealed record InsightCardReadModel(
    Guid Id,
    string Type,
    string Message,
    string SupportingData,
    string Status,
    DateOnly GeneratedAt);
