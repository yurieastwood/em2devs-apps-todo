using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class MultipleSkillTreesActiveTests
{
    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HoldMultipleSkillTreesConcurrently()
    {
        var profile = PlayerProfile.NewProfile();
        profile.DiscoverSkillTree(SkillTreeType.Creator);
        profile.DiscoverSkillTree(SkillTreeType.Builder);

        profile.SkillTrees.Count.ShouldBe(2);
        profile.SkillTrees.ShouldContain(t => t.Type == SkillTreeType.Creator);
        profile.SkillTrees.ShouldContain(t => t.Type == SkillTreeType.Builder);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ApplyProgressToEachTreeIndependently()
    {
        var profile = PlayerProfile.NewProfile();
        profile.DiscoverSkillTree(SkillTreeType.Creator);
        profile.DiscoverSkillTree(SkillTreeType.Builder);

        profile.RecordSkillTreeProgress(SkillTreeType.Creator);
        profile.RecordSkillTreeProgress(SkillTreeType.Builder);

        profile.SkillTrees.Single(t => t.Type == SkillTreeType.Creator)
            .TasksCompletedInTier.ShouldBe(1);
        profile.SkillTrees.Single(t => t.Type == SkillTreeType.Builder)
            .TasksCompletedInTier.ShouldBe(1);
    }
}
