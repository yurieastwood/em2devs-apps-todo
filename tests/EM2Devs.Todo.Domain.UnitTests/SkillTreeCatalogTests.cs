using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class SkillTreeCatalogTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeEveryTreeType_When_Built()
    {
        var catalog = SkillTreeCatalog.Build(Array.Empty<SkillTree>());

        catalog.Entries.Count.ShouldBe(Enum.GetValues<SkillTreeType>().Length);
        foreach (SkillTreeType type in Enum.GetValues<SkillTreeType>())
        {
            catalog.Entries.ShouldContain(e => e.Type == type);
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowUnlockedEntryForDiscoveredTree()
    {
        var tree = SkillTree.Discover(SkillTreeType.Builder);
        var catalog = SkillTreeCatalog.Build(new[] { tree });

        var builder = catalog.Entries.Single(e => e.Type == SkillTreeType.Builder);
        builder.IsUnlocked.ShouldBeTrue();
        builder.UnlockedTree.ShouldBe(tree);
        builder.UnlockHint.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowLockedSilhouetteWithHint_When_NotYetDiscovered()
    {
        var catalog = SkillTreeCatalog.Build(Array.Empty<SkillTree>());

        var guardian = catalog.Entries.Single(e => e.Type == SkillTreeType.Guardian);
        guardian.IsUnlocked.ShouldBeFalse();
        guardian.UnlockedTree.ShouldBeNull();
        guardian.UnlockHint.ShouldNotBeNullOrWhiteSpace();
        guardian.UnlockHint!.ShouldContain("15");
        guardian.UnlockHint.ShouldContain("health");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ContainUnlockHintForEachTreeType()
    {
        var catalog = SkillTreeCatalog.Build(Array.Empty<SkillTree>());
        foreach (var entry in catalog.Entries)
        {
            entry.UnlockHint.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_InputNull()
    {
        Should.Throw<ArgumentNullException>(() => SkillTreeCatalog.Build(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_UnlockedTreeNull()
    {
        Should.Throw<ArgumentNullException>(() => SkillTreeCatalogEntry.Unlocked(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_LockedHintEmpty()
    {
        Should.Throw<DomainException>(() =>
            SkillTreeCatalogEntry.Locked(SkillTreeType.Creator, ""));
        Should.Throw<DomainException>(() =>
            SkillTreeCatalogEntry.Locked(SkillTreeType.Creator, "  "));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_BuildCalledWithNullList()
    {
        var ex = Should.Throw<ArgumentNullException>(() => SkillTreeCatalog.Build(null!));
        ex.ParamName.ShouldBe("unlockedTrees");
    }
}
