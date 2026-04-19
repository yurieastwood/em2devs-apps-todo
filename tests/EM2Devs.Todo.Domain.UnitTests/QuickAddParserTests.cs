using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using EM2Devs.Todo.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class QuickAddParserTests
{
    private static readonly DateOnly _today = new(2026, 4, 10);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseTitleTagPriorityAndDate_When_GivenFullInput()
    {
        var result = QuickAddParser.Parse("Submit tax return #personal !high ^April 15", _today);

        result.Title.Value.ShouldBe("Submit tax return");
        result.Tags.ShouldContain(Tag.From("personal"));
        result.Tags.Count.ShouldBe(1);
        result.Priority.ShouldBe(TaskPriority.High);
        result.DueDate.ShouldBe(new DateOnly(2026, 4, 15));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RollOverToNextYear_When_MonthDayIsBeforeToday()
    {
        var result = QuickAddParser.Parse("Do thing ^April 1", _today);
        result.DueDate.ShouldBe(new DateOnly(2027, 4, 1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SupportMultipleTags_When_MultipleHashDirectives()
    {
        var result = QuickAddParser.Parse("Plan trip #travel #personal", _today);
        result.Tags.Count.ShouldBe(2);
        result.Tags.ShouldContain(Tag.From("travel"));
        result.Tags.ShouldContain(Tag.From("personal"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseNaturalLanguageDate_When_CaretPrefixesTomorrow()
    {
        var result = QuickAddParser.Parse("Buy milk ^tomorrow", _today);
        result.DueDate.ShouldBe(_today.AddDays(1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoTagsPriorityOrDate_When_OnlyTitleGiven()
    {
        var result = QuickAddParser.Parse("Just a title", _today);
        result.Title.Value.ShouldBe("Just a title");
        result.Tags.ShouldBeEmpty();
        result.Priority.ShouldBeNull();
        result.DueDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TerminateMultiWordDate_When_NextDirectiveAppears()
    {
        // date "April 15" is followed by another directive (#tag)
        var result = QuickAddParser.Parse("Do work ^April 15 #work", _today);
        result.DueDate.ShouldBe(new DateOnly(2026, 4, 15));
        result.Tags.ShouldContain(Tag.From("work"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InputEmpty()
    {
        Should.Throw<DomainException>(() => QuickAddParser.Parse("", _today));
        Should.Throw<DomainException>(() => QuickAddParser.Parse("   ", _today));
        Should.Throw<DomainException>(() => QuickAddParser.Parse(null!, _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NoTitleTokens()
    {
        Should.Throw<DomainException>(() => QuickAddParser.Parse("#tag !high", _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_PriorityUnknown()
    {
        Should.Throw<DomainException>(() => QuickAddParser.Parse("Do thing !urgent", _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IgnoreBareDirectiveTokens_When_NoValueAfterPrefix()
    {
        // A bare "#", "!" or "^" is treated as title content rather than a directive.
        var result = QuickAddParser.Parse("Task # ! ^ end", _today);
        result.Title.Value.ShouldBe("Task # ! ^ end");
        result.Tags.ShouldBeEmpty();
        result.Priority.ShouldBeNull();
        result.DueDate.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SupportLowPriority_When_Given()
    {
        var result = QuickAddParser.Parse("Thing !low", _today);
        result.Priority.ShouldBe(TaskPriority.Low);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectNullTitleOrTags_InResult()
    {
        Should.Throw<ArgumentNullException>(() =>
            new QuickAddResult(null!, Array.Empty<Tag>(), null, null));
        Should.Throw<ArgumentNullException>(() =>
            new QuickAddResult(new TaskTitle("x"), null!, null, null));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseLatestDate_When_MultipleDateDirectivesPresent()
    {
        // Two ^date directives — second should win. If dateTokens.Clear() is removed,
        // stale tokens from the first date would corrupt the second parse.
        var result = QuickAddParser.Parse("buy groceries ^tomorrow ^in 3 days", _today);
        result.DueDate.ShouldBe(_today.AddDays(3));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseFullMonthName_When_DateUsesFullMonth()
    {
        // "February 14" — only parseable via "MMMM d" format.
        // If that format is removed (mutation), only "Feb 14" short form would work.
        var result = QuickAddParser.Parse("dinner plans ^February 14", _today);
        result.DueDate.ShouldNotBeNull();
        result.DueDate!.Value.Month.ShouldBe(2);
        result.DueDate.Value.Day.ShouldBe(14);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseAbbreviatedMonthName_When_DateUsesShortMonth()
    {
        // "Feb 14" — only parseable via "MMM d" format.
        // If that format is removed (mutation), "Feb 14" would fail while "February 14" still works.
        var result = QuickAddParser.Parse("dinner plans ^Feb 14", _today);
        result.DueDate.ShouldNotBeNull();
        result.DueDate!.Value.Month.ShouldBe(2);
        result.DueDate.Value.Day.ShouldBe(14);
    }

    [Theory]
    [Trait("Category", "Domain")]
    [InlineData("daily", RecurrencePattern.Daily)]
    [InlineData("weekly", RecurrencePattern.Weekly)]
    [InlineData("monthly", RecurrencePattern.Monthly)]
    [InlineData("Daily", RecurrencePattern.Daily)]
    public void Should_ParseRepeatPattern_When_TildeDirectiveGiven(string pattern, RecurrencePattern expected)
    {
        var result = QuickAddParser.Parse($"standup notes ~{pattern}", _today);
        result.Title.Value.ShouldBe("standup notes");
        result.RepeatPattern.ShouldBe(expected);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_RepeatPatternUnknown()
    {
        var ex = Should.Throw<DomainException>(() => QuickAddParser.Parse("task ~biweekly", _today));
        ex.Message.ShouldContain("biweekly");
        ex.Message.ShouldContain("daily, weekly, or monthly");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveNoRepeatPattern_When_NoTildeDirective()
    {
        var result = QuickAddParser.Parse("normal task #work", _today);
        result.RepeatPattern.ShouldBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_TreatBareTilde_AsTitleContent()
    {
        var result = QuickAddParser.Parse("Task ~ end", _today);
        result.Title.Value.ShouldBe("Task ~ end");
        result.RepeatPattern.ShouldBeNull();
    }
}
