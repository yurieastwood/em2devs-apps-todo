using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class SkillTreePerkTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTipsPerk_When_Tier1()
    {
        var perk = SkillTreePerkCatalog.PerkFor(SkillTreeType.Guardian, new SkillTier(1));

        perk.Type.ShouldBe(SkillTreePerkType.Tips);
        perk.Tree.ShouldBe(SkillTreeType.Guardian);
        perk.Tier.Value.ShouldBe(1);
        perk.Description.ShouldContain("Guardian");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnWorkflowPerk_When_Tier2()
    {
        var perk = SkillTreePerkCatalog.PerkFor(SkillTreeType.Architect, new SkillTier(2));

        perk.Type.ShouldBe(SkillTreePerkType.Workflow);
        perk.Description.ShouldContain("quest templates");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnCosmeticPerk_When_Tier3()
    {
        var perk = SkillTreePerkCatalog.PerkFor(SkillTreeType.Creator, new SkillTier(3));

        perk.Type.ShouldBe(SkillTreePerkType.Cosmetic);
        perk.Description.ShouldContain("Creator");
        perk.Description.ShouldContain("badge");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnAllThreeTiers_When_AllPerksForTreeRequested()
    {
        var perks = SkillTreePerkCatalog.AllPerksFor(SkillTreeType.Scholar);

        perks.Count.ShouldBe(3);
        perks.Select(p => p.Type).ToList().ShouldBe(new List<SkillTreePerkType>
        {
            SkillTreePerkType.Tips,
            SkillTreePerkType.Workflow,
            SkillTreePerkType.Cosmetic
        });
        perks.Select(p => p.Tier.Value).ToList().ShouldBe(new List<int> { 1, 2, 3 });
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_PerkForTierNull()
    {
        Should.Throw<ArgumentNullException>(() =>
            SkillTreePerkCatalog.PerkFor(SkillTreeType.Builder, null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_DescriptionEmpty()
    {
        Should.Throw<DomainException>(() =>
            new SkillTreePerk(SkillTreeType.Creator, new SkillTier(1), SkillTreePerkType.Tips, ""));
        Should.Throw<DomainException>(() =>
            new SkillTreePerk(SkillTreeType.Creator, new SkillTier(1), SkillTreePerkType.Tips, "   "));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TierNullOnPerk()
    {
        Should.Throw<ArgumentNullException>(() =>
            new SkillTreePerk(SkillTreeType.Creator, null!, SkillTreePerkType.Tips, "desc"));
    }
}
