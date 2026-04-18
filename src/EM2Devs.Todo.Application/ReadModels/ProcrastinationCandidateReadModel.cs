namespace EM2Devs.Todo.Application.ReadModels;

public sealed record ProcrastinationCandidateReadModel(
    Guid TaskId,
    string Title,
    int UrgencyScore,
    IReadOnlyList<string> Signals,
    IReadOnlyList<InterventionOptionReadModel> Interventions);

public sealed record InterventionOptionReadModel(
    string Type,
    string Description);
