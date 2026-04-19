namespace EM2Devs.Todo.Application.ReadModels;

public sealed record SubscriptionReadModel(
    string Tier,
    string Status,
    bool IsPremium,
    bool IsActive,
    DateTimeOffset? ExpiresAt,
    bool AutoRenew);
