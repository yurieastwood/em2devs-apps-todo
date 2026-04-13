using Shouldly;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for ReviewSchedule value object.
/// Maps to: docs/features/reflection/weekly-review.feature
/// Rule: "The weekly review is prompted at a consistent user-chosen time"
/// </summary>
public sealed class ReviewScheduleTests
{
    // ── Scenario: Default review schedule when no preference is set ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_DefaultToSundayAt6PM_When_NoPreferenceSet()
    {
        // Given/When
        ReviewSchedule schedule = ReviewSchedule.Default;

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(18, 0));
    }

    // ── Scenario: Configure weekly review schedule ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseSaturdayAt10AM_When_ValidInput()
    {
        // Given/When
        ReviewSchedule schedule = ReviewSchedule.Parse("Saturday at 10 AM");

        // Then
        schedule.DayOfWeek.ShouldBe(DayOfWeek.Saturday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(10, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseSundayAt7PM_When_ValidInput()
    {
        ReviewSchedule schedule = ReviewSchedule.Parse("Sunday at 7 PM");

        schedule.DayOfWeek.ShouldBe(DayOfWeek.Sunday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(19, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CreateDirectly_When_DayAndTimeProvided()
    {
        ReviewSchedule schedule = new(DayOfWeek.Wednesday, new TimeOnly(14, 30));

        schedule.DayOfWeek.ShouldBe(DayOfWeek.Wednesday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(14, 30));
    }

    // ── Validation ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_EmptyScheduleText()
    {
        Should.Throw<DomainException>(() => ReviewSchedule.Parse(""));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_WhitespaceScheduleText()
    {
        Should.Throw<DomainException>(() => ReviewSchedule.Parse("   "));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_NoAtKeyword()
    {
        Should.Throw<DomainException>(() => ReviewSchedule.Parse("Saturday 10 AM"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvalidDayOfWeek()
    {
        Should.Throw<DomainException>(() => ReviewSchedule.Parse("Funday at 10 AM"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvalidTime()
    {
        Should.Throw<DomainException>(() => ReviewSchedule.Parse("Saturday at 25:00"));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowDomainException_When_InvalidDayOfWeekEnum()
    {
        Should.Throw<DomainException>(() => new ReviewSchedule((DayOfWeek)99, new TimeOnly(10, 0)));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveValueEquality_When_SameDayAndTime()
    {
        ReviewSchedule a = new(DayOfWeek.Monday, new TimeOnly(9, 0));
        ReviewSchedule b = new(DayOfWeek.Monday, new TimeOnly(9, 0));

        a.ShouldBe(b);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBeEqual_When_DifferentDayOrTime()
    {
        ReviewSchedule a = new(DayOfWeek.Monday, new TimeOnly(9, 0));
        ReviewSchedule b = new(DayOfWeek.Tuesday, new TimeOnly(9, 0));

        a.ShouldNotBe(b);
    }

    // ── Mutation-killing: DomainException message verification ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_EmptyScheduleText()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse(""));
        ex.Message.ShouldContain("Schedule text cannot be empty");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_NoAtKeyword()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse("Saturday 10 AM"));
        ex.Message.ShouldContain("Invalid schedule format");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_InvalidDay()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse("Funday at 10 AM"));
        ex.Message.ShouldContain("Invalid day of week");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_InvalidTime()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse("Saturday at 25:00"));
        ex.Message.ShouldContain("Invalid time");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithCorrectMessage_When_InvalidDayOfWeekEnum()
    {
        DomainException ex = Should.Throw<DomainException>(() => new ReviewSchedule((DayOfWeek)99, new TimeOnly(10, 0)));
        ex.Message.ShouldContain("Invalid day of week");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseMondayAt9AM_When_ValidInput()
    {
        ReviewSchedule schedule = ReviewSchedule.Parse("Monday at 9 AM");

        schedule.DayOfWeek.ShouldBe(DayOfWeek.Monday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(9, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithInputInMessage_When_InvalidFormat()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse("bad input"));
        ex.Message.ShouldContain("bad input");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithDayInMessage_When_InvalidDay()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse("Funday at 10 AM"));
        ex.Message.ShouldContain("Funday");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWithTimeInMessage_When_InvalidTime()
    {
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse("Saturday at 25:00"));
        ex.Message.ShouldContain("25:00");
    }

    // ── Mutation-killing: atIndex < 0 vs atIndex <= 0 boundary ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowFormatError_When_NoAtSeparatorFound()
    {
        // "at 10 AM" (no " at " with surrounding spaces) => atIndex is -1 => invalid format
        DomainException ex = Should.Throw<DomainException>(() => ReviewSchedule.Parse("at 10 AM"));
        ex.Message.ShouldContain("Invalid schedule format");
    }

    // ── Mutation-killing: ignoreCase parameter ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseLowercaseDay_When_CaseInsensitive()
    {
        ReviewSchedule schedule = ReviewSchedule.Parse("saturday at 10 AM");

        schedule.DayOfWeek.ShouldBe(DayOfWeek.Saturday);
        schedule.TimeOfDay.ShouldBe(new TimeOnly(10, 0));
    }

    // ── Mutation-killing: time format strings ──

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseSingleDigitHour_When_FormatIsHtt()
    {
        // Tests "h tt" format
        ReviewSchedule schedule = ReviewSchedule.Parse("Monday at 9 AM");

        schedule.TimeOfDay.ShouldBe(new TimeOnly(9, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseSingleDigitHourWithMinutes_When_FormatIsHmmtt()
    {
        // Tests "h:mm tt" format
        ReviewSchedule schedule = ReviewSchedule.Parse("Monday at 9:30 AM");

        schedule.TimeOfDay.ShouldBe(new TimeOnly(9, 30));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseDoubleDigitHour_When_FormatIsHHtt()
    {
        // Tests "hh tt" format
        ReviewSchedule schedule = ReviewSchedule.Parse("Monday at 10 AM");

        schedule.TimeOfDay.ShouldBe(new TimeOnly(10, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ParseDoubleDigitHourWithMinutes_When_FormatIsHHmmtt()
    {
        // Tests "hh:mm tt" format
        ReviewSchedule schedule = ReviewSchedule.Parse("Monday at 10:30 AM");

        schedule.TimeOfDay.ShouldBe(new TimeOnly(10, 30));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Parse24HourSingleDigit_When_FormatIsHmm()
    {
        // Tests "H:mm" format
        ReviewSchedule schedule = ReviewSchedule.Parse("Monday at 9:00");

        schedule.TimeOfDay.ShouldBe(new TimeOnly(9, 0));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_Parse24HourDoubleDigit_When_FormatIsHHmm()
    {
        // Tests "HH:mm" format
        ReviewSchedule schedule = ReviewSchedule.Parse("Monday at 14:30");

        schedule.TimeOfDay.ShouldBe(new TimeOnly(14, 30));
    }
}
