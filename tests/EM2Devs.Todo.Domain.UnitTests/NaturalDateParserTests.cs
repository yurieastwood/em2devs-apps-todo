using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.Services;
using Shouldly;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

public sealed class NaturalDateParserTests
{
    // Wednesday
    private static readonly DateOnly _today = new(2026, 4, 15);

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnToday_When_ExpressionIsToday()
    {
        NaturalDateParser.Parse("today", _today).ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTomorrow_When_ExpressionIsTomorrow()
    {
        NaturalDateParser.Parse("tomorrow", _today).ShouldBe(_today.AddDays(1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_BeCaseInsensitive_When_ParsingKeywords()
    {
        NaturalDateParser.Parse("ToMoRrOw", _today).ShouldBe(_today.AddDays(1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResolveNextWeekday_When_PrefixedWithNext()
    {
        // _today is Wed 2026-04-15. "next Tuesday" should be 2026-04-21.
        NaturalDateParser.Parse("next Tuesday", _today).ShouldBe(new DateOnly(2026, 4, 21));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_SkipToFollowingWeek_When_NextWeekdayIsToday()
    {
        // _today is Wed. "next Wednesday" must move forward 7 days, not return today.
        NaturalDateParser.Parse("next Wednesday", _today).ShouldBe(_today.AddDays(7));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ResolveUpcomingWeekday_When_WeekdayIsAlone()
    {
        // Bare "Friday" from Wed -> coming Friday
        NaturalDateParser.Parse("Friday", _today).ShouldBe(new DateOnly(2026, 4, 17));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnSameDay_When_WeekdayIsAloneAndMatchesToday()
    {
        // Bare "Wednesday" from Wed -> today
        NaturalDateParser.Parse("Wednesday", _today).ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AddDays_When_ExpressionIsInNDays()
    {
        NaturalDateParser.Parse("in 3 days", _today).ShouldBe(_today.AddDays(3));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptSingularDay_When_InOneDay()
    {
        NaturalDateParser.Parse("in 1 day", _today).ShouldBe(_today.AddDays(1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptZeroDays_When_InZeroDays()
    {
        NaturalDateParser.Parse("in 0 days", _today).ShouldBe(_today);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExpressionIsEmpty()
    {
        Should.Throw<DomainException>(() => NaturalDateParser.Parse("", _today));
        Should.Throw<DomainException>(() => NaturalDateParser.Parse("   ", _today));
        Should.Throw<DomainException>(() => NaturalDateParser.Parse(null!, _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_ExpressionUnrecognised()
    {
        Should.Throw<DomainException>(() => NaturalDateParser.Parse("banana", _today));
        Should.Throw<DomainException>(() => NaturalDateParser.Parse("next banana", _today));
        Should.Throw<DomainException>(() => NaturalDateParser.Parse("in three days", _today));
        Should.Throw<DomainException>(() => NaturalDateParser.Parse("in -1 days", _today));
        Should.Throw<DomainException>(() => NaturalDateParser.Parse("in 3 weeks", _today));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnTrueAndValue_When_TryParseSucceeds()
    {
        NaturalDateParser.TryParse("tomorrow", _today, out DateOnly value).ShouldBeTrue();
        value.ShouldBe(_today.AddDays(1));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnFalse_When_TryParseFails()
    {
        NaturalDateParser.TryParse("nonsense", _today, out DateOnly value).ShouldBeFalse();
        value.ShouldBe(default);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NextFollowedByUnknownWord()
    {
        // "next foo" — enters the "next " branch, fails weekday parse, must throw the
        // specific "Unrecognised weekday after 'next'" message (not the generic fallback).
        var ex = Should.Throw<DomainException>(() => NaturalDateParser.Parse("next foo", _today));
        ex.Message.ShouldContain("Unrecognised weekday after 'next'");
        ex.Message.ShouldContain("foo");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InClauseHasUnknownUnit()
    {
        // "in 5 hours" — enters "in " branch, but unit != day/days. Must throw the
        // specific "Unrecognised 'in N days' expression" message (not the generic fallback).
        var ex = Should.Throw<DomainException>(() => NaturalDateParser.Parse("in 5 hours", _today));
        ex.Message.ShouldContain("Unrecognised 'in N days' expression");
        ex.Message.ShouldContain("in 5 hours");
    }
}
