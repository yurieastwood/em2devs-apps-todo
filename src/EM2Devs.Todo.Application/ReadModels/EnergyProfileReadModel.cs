namespace EM2Devs.Todo.Application.ReadModels;

public sealed record EnergyProfileReadModel(
    string? CurrentLevel,
    bool HasSufficientData,
    double ConfidenceScore,
    string ConfidenceLevel,
    int DataPoints,
    string? InsufficientDataMessage);
