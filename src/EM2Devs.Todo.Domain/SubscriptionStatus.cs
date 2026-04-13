namespace EM2Devs.Todo.Domain;

/// <summary>
/// Status of a subscription lifecycle.
/// </summary>
public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled,
    GracePeriod
}
