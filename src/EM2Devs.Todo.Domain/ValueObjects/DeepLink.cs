using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a deep link to an entity, enabling tap-to-navigate from a notification.
/// </summary>
public sealed record DeepLink
{
    public string EntityType { get; }
    public string EntityId { get; }

    private DeepLink(string entityType, string entityId)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public static DeepLink Create(string entityType, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new DomainException("Deep link entity type cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new DomainException("Deep link entity id cannot be empty.");
        }

        return new DeepLink(entityType.Trim(), entityId.Trim());
    }

    /// <summary>
    /// Returns a path representation of the deep link suitable for routing.
    /// </summary>
    public string ToPath()
    {
        return $"/{EntityType}/{EntityId}";
    }
}
