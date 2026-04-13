using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// Represents a one-time cosmetic purchase (theme, avatar border, etc.).
/// Cosmetics are retained permanently, even after subscription downgrade.
/// Cosmetics provide no XP or gameplay advantage.
/// </summary>
public sealed record CosmeticPurchase
{
    public CosmeticPurchaseId Id { get; }
    public string ItemName { get; }
    public DateTimeOffset PurchasedAt { get; }
    public decimal Price { get; }

    public CosmeticPurchase(CosmeticPurchaseId id, string itemName, DateTimeOffset purchasedAt, decimal price)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(itemName))
        {
            throw new DomainException("Cosmetic item name cannot be empty.");
        }

        if (price < 0)
        {
            throw new DomainException("Cosmetic price cannot be negative.");
        }

        ItemName = itemName;
        PurchasedAt = purchasedAt;
        Price = price;
    }

    /// <summary>
    /// Factory method to create a new cosmetic purchase.
    /// </summary>
    public static CosmeticPurchase Create(string itemName, DateTimeOffset now, decimal price)
    {
        return new CosmeticPurchase(CosmeticPurchaseId.New(), itemName, now, price);
    }
}
