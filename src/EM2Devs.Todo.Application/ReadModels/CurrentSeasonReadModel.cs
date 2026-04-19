namespace EM2Devs.Todo.Application.ReadModels;

public sealed record CurrentSeasonReadModel(
    string Name,
    string Theme,
    DateOnly StartDate,
    DateOnly EndDate,
    int DaysRemaining,
    bool IsActive,
    SeasonalQuestLineReadModel QuestLine,
    IReadOnlyList<CosmeticItemReadModel> AvailableCosmetics);

public sealed record SeasonalQuestLineReadModel(
    int TotalStages,
    int CurrentStage,
    int TasksCompletedInStage,
    int TasksRemaining,
    bool IsCompleted);

public sealed record CosmeticItemReadModel(
    string Name,
    string Rarity,
    int RequiredStage,
    bool IsEarned);
