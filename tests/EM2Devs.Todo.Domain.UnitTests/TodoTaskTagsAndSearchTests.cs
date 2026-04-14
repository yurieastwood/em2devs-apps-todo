using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class TodoTaskTagsAndSearchTests
{
    private static TodoTask NewTask(string title = "Write report") =>
        TodoTask.Create(new TaskTitle(title));

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddTag_When_AddTagCalled()
    {
        var task = NewTask();
        task.AddTag(Tag.From("work"));

        task.Tags.ShouldContain(Tag.From("work"));
        task.HasTag(Tag.From("work")).ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IgnoreDuplicateTag_When_AlreadyPresent()
    {
        var task = NewTask();
        task.AddTag(Tag.From("work"));
        task.AddTag(Tag.From("WORK"));

        task.Tags.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RemoveTag_When_RemoveTagCalled()
    {
        var task = NewTask();
        task.AddTag(Tag.From("work"));
        task.RemoveTag(Tag.From("work"));

        task.Tags.ShouldBeEmpty();
        task.HasTag(Tag.From("work")).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeNoOp_When_RemovingAbsentTag()
    {
        var task = NewTask();
        task.RemoveTag(Tag.From("other"));
        task.Tags.ShouldBeEmpty();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowArgumentNullException_When_TagNull()
    {
        var task = NewTask();
        Should.Throw<ArgumentNullException>(() => task.AddTag(null!));
        Should.Throw<ArgumentNullException>(() => task.RemoveTag(null!));
        Should.Throw<ArgumentNullException>(() => task.HasTag(null!));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MatchKeywordInTitle_When_Searched()
    {
        var task = NewTask("Quarterly report draft");
        task.MatchesKeyword("REPORT").ShouldBeTrue();
        task.MatchesKeyword("report").ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_MatchKeywordInDescription_When_Searched()
    {
        var task = NewTask("Unrelated");
        task.UpdateDescription("Final report for Q2");
        task.MatchesKeyword("report").ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotMatchKeyword_When_NotPresent()
    {
        var task = NewTask("Pay bills");
        task.UpdateDescription("Water & electricity");
        task.MatchesKeyword("report").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_KeywordEmpty()
    {
        var task = NewTask();
        task.MatchesKeyword("").ShouldBeFalse();
        task.MatchesKeyword("   ").ShouldBeFalse();
        task.MatchesKeyword(null!).ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotMatchDescription_When_DescriptionIsNull()
    {
        var task = NewTask("Clean desk");
        task.MatchesKeyword("zzz").ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AssignAndClearQuestId()
    {
        var task = NewTask();
        var quest = QuestId.New();
        task.AssignToQuest(quest);
        task.AssignedQuestId.ShouldBe(quest);

        task.AssignToQuest(null);
        task.AssignedQuestId.ShouldBeNull();
    }
}
