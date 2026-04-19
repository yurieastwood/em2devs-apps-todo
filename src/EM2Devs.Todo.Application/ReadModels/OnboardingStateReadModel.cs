namespace EM2Devs.Todo.Application.ReadModels;

public sealed record OnboardingStateReadModel(
    IReadOnlyList<string> UnlockedFeatures,
    bool IsGamificationActive,
    IReadOnlyList<FeaturePreviewReadModel> UpcomingFeatures);

public sealed record FeaturePreviewReadModel(
    string Feature,
    string Description,
    string UnlockRequirement);
