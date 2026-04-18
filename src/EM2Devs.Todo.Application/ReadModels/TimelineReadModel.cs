namespace EM2Devs.Todo.Application.ReadModels;

public sealed record TimelineReadModel(
    IReadOnlyList<TimelineEventReadModel> Events,
    bool HasMore,
    Guid? NextCursor);

public sealed record TimelineEventReadModel(
    Guid Id,
    string EventType,
    DateTimeOffset OccurredAt,
    string Details,
    string? Note);
