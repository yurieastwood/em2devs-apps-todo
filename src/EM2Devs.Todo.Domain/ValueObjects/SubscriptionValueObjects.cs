namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Strongly-typed subscription identifier (ADR-023).
/// </summary>
public sealed record SubscriptionId(Guid Value)
{
    public static SubscriptionId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed team workspace identifier (ADR-023).
/// </summary>
public sealed record TeamWorkspaceId(Guid Value)
{
    public static TeamWorkspaceId New() => new(Guid.NewGuid());
}

/// <summary>
/// Strongly-typed cosmetic purchase identifier (ADR-023).
/// </summary>
public sealed record CosmeticPurchaseId(Guid Value)
{
    public static CosmeticPurchaseId New() => new(Guid.NewGuid());
}
