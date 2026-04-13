using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for the CosmeticPurchase value object.
/// Maps to: docs/features/monetisation/subscription-tiers.feature
/// Scenarios: "Purchase a cosmetic item", "Cosmetics do not affect gameplay",
///            "Cosmetics retained after downgrade"
/// </summary>
public sealed class CosmeticPurchaseTests
{
    private static readonly DateTimeOffset _now = new(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);

    // --- Scenario: Purchase a cosmetic item ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCosmeticPurchase_When_ValidParameters()
    {
        var id = CosmeticPurchaseId.New();
        var purchase = new CosmeticPurchase(id, "Midnight Theme", _now, 4.99m);

        purchase.Id.ShouldBe(id);
        purchase.ItemName.ShouldBe("Midnight Theme");
        purchase.PurchasedAt.ShouldBe(_now);
        purchase.Price.ShouldBe(4.99m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateCosmeticPurchaseViaFactory_When_ValidParameters()
    {
        var purchase = CosmeticPurchase.Create("Midnight Theme", _now, 4.99m);

        purchase.Id.ShouldNotBeNull();
        purchase.ItemName.ShouldBe("Midnight Theme");
        purchase.PurchasedAt.ShouldBe(_now);
        purchase.Price.ShouldBe(4.99m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowFreeCosmetic_When_PriceIsZero()
    {
        var purchase = CosmeticPurchase.Create("Free Badge", _now, 0m);

        purchase.Price.ShouldBe(0m);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_IdIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => new CosmeticPurchase(null!, "Theme", _now, 4.99m));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ItemNameIsEmpty()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticPurchase(CosmeticPurchaseId.New(), "", _now, 4.99m));
        ex.Message.ShouldContain("Cosmetic item name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ItemNameIsWhitespace()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticPurchase(CosmeticPurchaseId.New(), "  ", _now, 4.99m));
        ex.Message.ShouldContain("Cosmetic item name cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PriceIsNegative()
    {
        var ex = Should.Throw<DomainException>(
            () => new CosmeticPurchase(CosmeticPurchaseId.New(), "Theme", _now, -1m));
        ex.Message.ShouldContain("Cosmetic price cannot be negative");
    }

    // --- Scenario: Cosmetics do not affect gameplay ---
    // (Cosmetic purchases have no XP or gameplay-related properties by design.
    //  This is tested via PremiumFeatureGateTests — cosmetics are purely visual.)

    // --- Scenario: Cosmetics retained after downgrade ---
    // (CosmeticPurchase is a standalone value object — it has no subscription dependency.
    //  Retained permanently by design. No method to revoke.)
}
