using Shouldly;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;
using Xunit;

namespace EM2Devs.Todo.Domain.UnitTests;

/// <summary>
/// Scenario-driven tests for AnnualWrapped entity.
/// Maps to: docs/features/reflection/annual-wrapped.feature
/// </summary>
public sealed class AnnualWrappedTests
{
    private static readonly DateOnly _dec15 = new(2026, 12, 15);
    private static readonly DateOnly _dec20 = new(2026, 12, 20);
    private static readonly DateOnly _janSignup = new(2026, 1, 1);
    private static readonly DateOnly _juneSignup = new(2026, 6, 1);
    private static readonly DateOnly _novSignup = new(2026, 11, 1);

    private static List<WrappedSlide> CreateStandardSlides() =>
    [
        new WrappedSlide("Total tasks completed", "156 tasks", "counter"),
        new WrappedSlide("Total XP earned", "12,500 XP", "counter"),
        new WrappedSlide("Levels gained", "Level 5 to Level 15", "progress"),
        new WrappedSlide("Longest streak", "42 days", "calendar"),
        new WrappedSlide("Quests completed", "8 quests", "counter"),
        new WrappedSlide("Hardest Boss Task", "Migrate database", "highlight"),
        new WrappedSlide("Most productive month", "March", "chart"),
        new WrappedSlide("Skill tree growth", "3 trees unlocked", "tree"),
        new WrappedSlide("Titles earned", "2 new titles", "badge"),
        new WrappedSlide("Top insight", "Creative task timing pattern", "card"),
        new WrappedSlide("Seasons participated in", "2 seasons", "timeline")
    ];

    // --- Scenario: Annual wrapped is generated after December 15 ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateWrapped_When_AfterDecember15WithSufficientData()
    {
        // Given it is December 15th or later in the current year
        // And I have used Waypoint for at least 3 months
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // Then I should see a multi-slide summary
        wrapped.Year.ShouldBe(2026);
        wrapped.Slides.Count.ShouldBe(11);
        wrapped.IsPartialYear.ShouldBeFalse();
        wrapped.DataStartDate.ShouldBeNull();
        wrapped.HasBranding.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateWrapped_When_ExactlyDecember15()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec15, _janSignup);

        wrapped.Year.ShouldBe(2026);
        wrapped.Slides.Count.ShouldBe(11);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenBeforeDecember15_When_GeneratingWrapped()
    {
        // Given it is before December 15
        var dec14 = new DateOnly(2026, 12, 14);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, dec14, _janSignup))
            .Message.ShouldContain("December 15");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenBeforeDecember_When_GeneratingWrapped()
    {
        var nov30 = new DateOnly(2026, 11, 30);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, nov30, _janSignup))
            .Message.ShouldContain("December 15");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AllowGenerationForPastYear_When_ViewingInNextYear()
    {
        // Viewing 2025 wrapped in January 2027 should work
        var jan2027 = new DateOnly(2027, 1, 15);
        var jan2025Signup = new DateOnly(2025, 1, 1);
        var slides = CreateStandardSlides();

        var wrapped = AnnualWrapped.Generate(2025, slides, jan2027, jan2025Signup);
        wrapped.Year.ShouldBe(2025);
    }

    // --- Scenario: Wrapped not available with insufficient data ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenInsufficientData_When_SignedUpRecently()
    {
        // Given I signed up in November and have only 6 weeks of data
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, _dec20, _novSignup))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowTeaserMessage_When_InsufficientDataForWrapped()
    {
        // Then I should see a message that my wrapped will be available next year
        // And I should see a teaser of what wrapped will include
        var (isAvailable, message) = AnnualWrapped.CheckAvailability(_dec20, _novSignup, 2026);

        isAvailable.ShouldBeFalse();
        message.ShouldContain("next year");
        message.ShouldContain("teaser");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowNotReadyMessage_When_BeforeDecember15()
    {
        var nov30 = new DateOnly(2026, 11, 30);
        var (isAvailable, message) = AnnualWrapped.CheckAvailability(nov30, _janSignup, 2026);

        isAvailable.ShouldBeFalse();
        message.ShouldContain("December 15");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowReadyMessage_When_AvailableAndSufficientData()
    {
        var (isAvailable, message) = AnnualWrapped.CheckAvailability(_dec20, _janSignup, 2026);

        isAvailable.ShouldBeTrue();
        message.ShouldContain("ready");
    }

    // --- Scenario: Slides with zero data show encouraging messaging ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ShowEncouragingMessage_When_SlideHasZeroData()
    {
        // Given I have not completed any quests this year
        // Then the "Quests completed" slide should not be hidden
        // And it should display an encouraging message
        var encouragingSlide = WrappedSlide.CreateEncouraging(
            "Quests completed",
            "No quests yet — your first quest awaits next year!",
            "counter");

        encouragingSlide.Title.ShouldBe("Quests completed");
        encouragingSlide.Metric.ShouldContain("No quests yet");
        encouragingSlide.VisualizationType.ShouldBe("counter");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeEncouragingSlidesInWrapped_When_Generated()
    {
        var slides = new List<WrappedSlide>
        {
            new("Total tasks completed", "156 tasks", "counter"),
            WrappedSlide.CreateEncouraging("Quests completed", "No quests yet — your first quest awaits next year!", "counter"),
            new("Longest streak", "42 days", "calendar")
        };

        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);
        wrapped.Slides.Count.ShouldBe(3);
        wrapped.Slides[1].Metric.ShouldContain("No quests yet");
    }

    // --- Scenario: Mid-year signup users receive a partial wrapped ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GeneratePartialWrapped_When_SignedUpMidYear()
    {
        // Given I signed up in June and have at least 3 months of data
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _juneSignup);

        // Then I should receive a "Year So Far" wrapped summary
        wrapped.IsPartialYear.ShouldBeTrue();
        // And it should cover only the months since my signup
        wrapped.DataStartDate.ShouldBe(_juneSignup);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NotBePartialYear_When_SignedUpInJanuary()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        wrapped.IsPartialYear.ShouldBeFalse();
        wrapped.DataStartDate.ShouldBeNull();
    }

    // --- Scenario: View wrapped as an interactive slideshow ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_StartAtFirstSlide_When_OpeningWrapped()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // Then I should see a slide-by-slide interactive presentation
        wrapped.CurrentSlideIndex.ShouldBe(0);
        wrapped.GetCurrentSlide().ShouldBe(slides[0]);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NavigateForward_When_NotAtLastSlide()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // And I should be able to navigate forward
        wrapped.NavigateForward();
        wrapped.CurrentSlideIndex.ShouldBe(1);
        wrapped.GetCurrentSlide().Title.ShouldBe("Total XP earned");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_NavigateBackward_When_NotAtFirstSlide()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // Navigate forward first, then backward
        wrapped.NavigateForward();
        wrapped.NavigateBackward();
        wrapped.CurrentSlideIndex.ShouldBe(0);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenNavigatingPastLastSlide_When_AtEnd()
    {
        var slides = new List<WrappedSlide>
        {
            new("Slide 1", "Data", "counter"),
            new("Slide 2", "Data", "counter")
        };
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        wrapped.NavigateForward(); // now at index 1 (last)

        Should.Throw<DomainException>(() => wrapped.NavigateForward())
            .Message.ShouldContain("last slide");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenNavigatingBeforeFirstSlide_When_AtStart()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.NavigateBackward())
            .Message.ShouldContain("first slide");
    }

    // --- Scenario: Share wrapped highlights ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateShareableSlide_When_Sharing()
    {
        // Given I am viewing my annual wrapped
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // When I choose to share a slide
        var shareable = wrapped.GetShareableSlide(0);

        // Then I should be able to generate a shareable image of that slide
        shareable.IsShareable.ShouldBeTrue();
        shareable.IsExcludedFromShare.ShouldBeFalse();

        // And the image should include Waypoint branding
        wrapped.HasBranding.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenSharingExcludedSlide_When_SlideExcluded()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        wrapped.ExcludeSlideFromShare(0);

        Should.Throw<DomainException>(() => wrapped.GetShareableSlide(0))
            .Message.ShouldContain("excluded");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenShareableSlideIndexOutOfRange_When_NegativeIndex()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.GetShareableSlide(-1))
            .Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenShareableSlideIndexOutOfRange_When_TooLarge()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.GetShareableSlide(100))
            .Message.ShouldContain("out of range");
    }

    // --- Scenario: View past year's wrapped ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LoadHistoricalWrapped_When_ViewingPastYear()
    {
        // Given I have a wrapped summary from last year
        var slides = CreateStandardSlides();
        var historical = AnnualWrapped.LoadHistorical(2025, slides, false, null);

        // Then I should see last year's wrapped available for replay
        historical.Year.ShouldBe(2025);
        historical.Slides.Count.ShouldBe(11);
        historical.IsPartialYear.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_LoadPartialHistoricalWrapped_When_PastYearWasPartial()
    {
        var slides = CreateStandardSlides();
        var dataStart = new DateOnly(2025, 6, 1);
        var historical = AnnualWrapped.LoadHistorical(2025, slides, true, dataStart);

        historical.IsPartialYear.ShouldBeTrue();
        historical.DataStartDate.ShouldBe(dataStart);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CompareYearOverYear_When_ViewingHistoricalWrapped()
    {
        // And I should be able to compare year-over-year statistics
        var slides2025 = new List<WrappedSlide> { new("Total tasks", "100 tasks", "counter") };
        var slides2026 = new List<WrappedSlide> { new("Total tasks", "156 tasks", "counter") };

        var wrapped2025 = AnnualWrapped.LoadHistorical(2025, slides2025, false, null);
        var wrapped2026 = AnnualWrapped.Generate(2026, slides2026, _dec20, _janSignup);

        wrapped2025.Year.ShouldBe(2025);
        wrapped2026.Year.ShouldBe(2026);
        wrapped2025.Slides[0].Metric.ShouldBe("100 tasks");
        wrapped2026.Slides[0].Metric.ShouldBe("156 tasks");
    }

    // --- Scenario: User can exclude specific data from shareable wrapped ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeSlideFromShare_When_UserSelectsExclusion()
    {
        // Given I am viewing my annual wrapped
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // When I choose to exclude a data point
        wrapped.ExcludeSlideFromShare(2);

        // Then the excluded data should still be visible in my private wrapped view
        wrapped.Slides[2].IsExcludedFromShare.ShouldBeTrue();
        wrapped.Slides[2].Title.ShouldBe("Levels gained");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeSlideBackInShare_When_UserReincludesIt()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        wrapped.ExcludeSlideFromShare(2);
        wrapped.Slides[2].IsExcludedFromShare.ShouldBeTrue();

        wrapped.IncludeSlideInShare(2);
        wrapped.Slides[2].IsExcludedFromShare.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenExcludeIndexOutOfRange_When_NegativeIndex()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.ExcludeSlideFromShare(-1))
            .Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenExcludeIndexOutOfRange_When_TooLarge()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.ExcludeSlideFromShare(100))
            .Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenIncludeIndexOutOfRange_When_NegativeIndex()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.IncludeSlideInShare(-1))
            .Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenIncludeIndexOutOfRange_When_TooLarge()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.IncludeSlideInShare(100))
            .Message.ShouldContain("out of range");
    }

    // --- Validation edge cases ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenNoSlides_When_GeneratingWrapped()
    {
        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, [], _dec20, _janSignup))
            .Message.ShouldContain("at least one slide");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenNoSlides_When_LoadingHistorical()
    {
        Should.Throw<DomainException>(() =>
            AnnualWrapped.LoadHistorical(2025, [], false, null))
            .Message.ShouldContain("at least one slide");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenSlidesNull_When_GeneratingWrapped()
    {
        Should.Throw<ArgumentNullException>(() =>
            AnnualWrapped.Generate(2026, null!, _dec20, _janSignup));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenSlidesNull_When_LoadingHistorical()
    {
        Should.Throw<ArgumentNullException>(() =>
            AnnualWrapped.LoadHistorical(2025, null!, false, null));
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveUniqueId_When_GeneratingWrapped()
    {
        var slides = CreateStandardSlides();
        var w1 = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);
        var w2 = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        w1.Id.ShouldNotBe(w2.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HaveUniqueId_When_LoadingHistorical()
    {
        var slides = CreateStandardSlides();
        var w1 = AnnualWrapped.LoadHistorical(2025, slides, false, null);
        var w2 = AnnualWrapped.LoadHistorical(2025, slides, false, null);

        w1.Id.ShouldNotBe(w2.Id);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnNotAvailable_When_CheckingEarlyMonth()
    {
        var oct15 = new DateOnly(2026, 10, 15);
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(oct15, _janSignup, 2026);
        isAvailable.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleSignupInSameMonth_When_CheckingAvailability()
    {
        // Signup in December, less than 3 months
        var decSignup = new DateOnly(2026, 12, 1);
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(_dec20, decSignup, 2026);
        isAvailable.ShouldBeFalse();
    }

    // --- Boundary tests for mutation coverage ---

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GenerateWrapped_When_ExactlyThreeMonthsOfData()
    {
        // Signup October 1 => exactly 3 months by Dec 20 (Oct, Nov, Dec = months 10-12, diff=2? No: Dec-Oct=2)
        // Actually CalculateMonthsOfData: effectiveStart=Oct 1, effectiveEnd=Dec 20, months = 12-10 = 2
        // That's < 3. Need signup in September.
        // Sep 1 signup: months = 12-9 = 3. Exactly 3. Should pass.
        var sepSignup = new DateOnly(2026, 9, 1);
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, sepSignup);

        wrapped.IsPartialYear.ShouldBeTrue();
        wrapped.DataStartDate.ShouldBe(sepSignup);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWrapped_When_ExactlyTwoMonthsOfData()
    {
        // Oct 1 signup: months = 12-10 = 2. Should fail.
        var octSignup = new DateOnly(2026, 10, 1);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, _dec20, octSignup))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenShareableSlideIndexExactlyAtCount_When_AtBoundary()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // slideIndex == _slides.Count should throw (>= Count)
        Should.Throw<DomainException>(() => wrapped.GetShareableSlide(slides.Count))
            .Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_GetShareableSlideAtLastIndex_When_ValidBoundary()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // slideIndex == _slides.Count - 1 should work
        var shareable = wrapped.GetShareableSlide(slides.Count - 1);
        shareable.ShouldNotBeNull();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenExcludeIndexExactlyAtCount_When_AtBoundary()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.ExcludeSlideFromShare(slides.Count))
            .Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ExcludeSlideAtLastIndex_When_ValidBoundary()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        wrapped.ExcludeSlideFromShare(slides.Count - 1);
        wrapped.Slides[slides.Count - 1].IsExcludedFromShare.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ThrowWhenIncludeIndexExactlyAtCount_When_AtBoundary()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        Should.Throw<DomainException>(() => wrapped.IncludeSlideInShare(slides.Count))
            .Message.ShouldContain("out of range");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeSlideAtLastIndex_When_ValidBoundary()
    {
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        wrapped.ExcludeSlideFromShare(slides.Count - 1);
        wrapped.IncludeSlideInShare(slides.Count - 1);
        wrapped.Slides[slides.Count - 1].IsExcludedFromShare.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckAvailabilityOnDec15_When_ExactBoundary()
    {
        // Exactly Dec 15 should be available (not < 15)
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(_dec15, _janSignup, 2026);
        isAvailable.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckAvailabilityOnDec14_When_OneDayBefore()
    {
        var dec14 = new DateOnly(2026, 12, 14);
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(dec14, _janSignup, 2026);
        isAvailable.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckAvailabilityExactlyThreeMonths_When_CheckingAvailability()
    {
        // Sep signup = exactly 3 months by Dec 20. Should be available.
        var sepSignup = new DateOnly(2026, 9, 1);
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(_dec20, sepSignup, 2026);
        isAvailable.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckAvailabilityTwoMonths_When_InsufficientData()
    {
        // Oct signup = 2 months by Dec 20. Should not be available.
        var octSignup = new DateOnly(2026, 10, 1);
        var (isAvailable, message) = AnnualWrapped.CheckAvailability(_dec20, octSignup, 2026);
        isAvailable.ShouldBeFalse();
        message.ShouldContain("next year");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleSignupBeforeYear_When_CalculatingMonths()
    {
        // Signup in 2025, viewing 2026 wrapped. effectiveStart should be Jan 1 2026.
        var earlySignup = new DateOnly(2025, 6, 1);
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, earlySignup);

        // Not partial year since signup is before the year
        wrapped.IsPartialYear.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleViewingPastYearWrapped_When_TodayIsNextYear()
    {
        // Viewing 2025 wrapped in Jan 2027. effectiveEnd should be Dec 31 2025.
        var jan2027 = new DateOnly(2027, 1, 15);
        var janSignup2025 = new DateOnly(2025, 1, 1);
        var slides = CreateStandardSlides();

        var wrapped = AnnualWrapped.Generate(2025, slides, jan2027, janSignup2025);
        wrapped.Year.ShouldBe(2025);
        wrapped.IsPartialYear.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleViewingPastYearWrapped_When_TodayIsSameYear()
    {
        // Viewing 2026 wrapped on Dec 20 2026. effectiveEnd should be Dec 20 (today).
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        // Should use today as effectiveEnd
        wrapped.Year.ShouldBe(2026);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroMonths_When_SignupAfterToday()
    {
        // Signup in future relative to wrapped eval date should produce insufficient data
        var futureSignup = new DateOnly(2027, 1, 1);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, _dec20, futureSignup))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleSignupOnJan1_When_CalculatingPartialYear()
    {
        // Signup on exactly Jan 1 should NOT be partial year (month == 1)
        var jan1Signup = new DateOnly(2026, 1, 1);
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, jan1Signup);

        wrapped.IsPartialYear.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_HandleSignupOnFeb1_When_CalculatingPartialYear()
    {
        // Signup on Feb 1 should be partial year (month > 1)
        var feb1Signup = new DateOnly(2026, 2, 1);
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, feb1Signup);

        wrapped.IsPartialYear.ShouldBeTrue();
        wrapped.DataStartDate.ShouldBe(feb1Signup);
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_RejectPastYearWrapped_When_SignupTooLateForPastYear()
    {
        // Viewing 2025 wrapped in Jan 2027, signup Oct 2025.
        // effectiveStart = Oct 2025, effectiveEnd = Dec 31 2025.
        // Months = 12-10 = 2 (< 3). Should fail.
        var jan2027 = new DateOnly(2027, 1, 15);
        var octSignup2025 = new DateOnly(2025, 10, 1);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2025, slides, jan2027, octSignup2025))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_AcceptPastYearWrapped_When_SignupEarlyEnoughForPastYear()
    {
        // Viewing 2025 wrapped in Jan 2027, signup Sep 2025.
        // effectiveStart = Sep 2025, effectiveEnd = Dec 31 2025.
        // Months = 12-9 = 3. Should pass.
        var jan2027 = new DateOnly(2027, 1, 15);
        var sepSignup2025 = new DateOnly(2025, 9, 1);
        var slides = CreateStandardSlides();

        var wrapped = AnnualWrapped.Generate(2025, slides, jan2027, sepSignup2025);
        wrapped.Year.ShouldBe(2025);
        wrapped.IsPartialYear.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseSignupDate_When_SignupAfterJan1()
    {
        // Signup March 1 2026 vs Jan 1 2026.
        // With signup March 1: months = 12-3 = 9.
        // If mutation always used yearStart (Jan 1): months = 12-1 = 11.
        // Both >= 3, so we need a scenario where it matters.
        // Signup Oct 20 2026: months = 12-10 = 2 < 3 => should fail.
        // If mutation used Jan 1: months = 12-1 = 11 >= 3 => would pass.
        var octSignup = new DateOnly(2026, 10, 20);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, _dec20, octSignup))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseYearEnd_When_ViewingPastYear()
    {
        // Viewing 2025 wrapped in Dec 2027, signup Jan 2025.
        // effectiveEnd should be Dec 31 2025 (not Dec 2027).
        // If mutation used today instead: months would be (2027-2025)*12 + 12-1 = 35.
        // With correct effectiveEnd (Dec 31 2025): months = 12-1 = 11.
        // Both >= 3, so we need a scenario where it matters.
        // Signup Nov 2025, viewing Jan 2027.
        // Correct: effectiveEnd = Dec 31 2025, effectiveStart = Nov 2025. Months = 12-11 = 1 < 3 => fail.
        // Mutated: effectiveEnd = Jan 2027. Months = (2027-2025)*12 + 1-11 = 24-10 = 14 >= 3 => pass.
        var jan2027 = new DateOnly(2027, 1, 15);
        var novSignup2025 = new DateOnly(2025, 11, 1);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2025, slides, jan2027, novSignup2025))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroMonths_When_EffectiveEndEqualsEffectiveStart()
    {
        // Signup Dec 20 2026, today Dec 20 2026. Months = 12-12 = 0 < 3.
        var decSignup = new DateOnly(2026, 12, 20);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, _dec20, decSignup))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckAvailabilityCorrectly_When_SignupBeforeYear()
    {
        // Signup in 2024, checking 2026 wrapped. effectiveStart should be Jan 1 2026.
        var earlySignup = new DateOnly(2024, 3, 15);
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(_dec20, earlySignup, 2026);
        isAvailable.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckAvailabilityForPastYear_When_ViewingFromNextYear()
    {
        // Checking 2025 wrapped from Jan 2027. Signup Nov 2025 => 1 month < 3.
        var jan2027 = new DateOnly(2027, 1, 15);
        var novSignup2025 = new DateOnly(2025, 11, 1);
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(jan2027, novSignup2025, 2025);

        isAvailable.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_CheckAvailabilityOnDec1_When_InDecemberButBefore15()
    {
        // Dec 1 is in December (month 12) but day < 15. Should not be available.
        var dec1 = new DateOnly(2026, 12, 1);
        var (isAvailable, _) = AnnualWrapped.CheckAvailability(dec1, _janSignup, 2026);
        isAvailable.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_IncludeSlideAtIndexZero_When_ValidBoundary()
    {
        // Kills mutation: slideIndex < 0 -> slideIndex <= 0 in IncludeSlideInShare
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, _janSignup);

        wrapped.ExcludeSlideFromShare(0);
        wrapped.Slides[0].IsExcludedFromShare.ShouldBeTrue();

        wrapped.IncludeSlideInShare(0);
        wrapped.Slides[0].IsExcludedFromShare.ShouldBeFalse();
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_ReturnZeroMonths_When_EffectiveEndBeforeEffectiveStart()
    {
        // Kills mutations: effectiveEnd < effectiveStart block removal and <= mutation
        // Signup Dec 25 2026, today Dec 20 2026. effectiveStart = Dec 25, effectiveEnd = Dec 20.
        // effectiveEnd < effectiveStart -> return 0. Without block: would calculate negative months.
        var futureInMonthSignup = new DateOnly(2026, 12, 25);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, _dec20, futureInMonthSignup))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseSignupMonth_When_SignupInSameYear()
    {
        // Kills mutation: signupDate.Year < year -> signupDate.Year <= year
        // Signup in same year (2026): startMonth = signupDate.Month, not 1.
        // Oct 2026 signup: startMonth = 10, endMonth = 12. months = 2 < 3. Should fail.
        // If mutated to <=, startMonth = 1, months = 11 >= 3. Would pass.
        var octSignup = new DateOnly(2026, 10, 1);
        var slides = CreateStandardSlides();

        Should.Throw<DomainException>(() =>
            AnnualWrapped.Generate(2026, slides, _dec20, octSignup))
            .Message.ShouldContain("Insufficient data");
    }

    [Fact]
    [Trait("Category", "Domain")]
    public void Should_UseMonth1_When_SignupBeforeYear()
    {
        // Kills mutation: signupDate.Year < year always false
        // Signup 2025, year 2026: startMonth should be 1 (Jan).
        // If mutated (never use 1), startMonth = signupDate.Month = 6.
        // endMonth = 12. months = 6. Both >= 3. Need a scenario where it matters:
        // Signup Nov 2025, year 2026. Correct: startMonth = 1, endMonth = 12, months = 11.
        // Mutated: startMonth = 11, endMonth = 12, months = 1 < 3.
        // Already tested via Should_HandleSignupBeforeYear_When_CalculatingMonths.
        // Let's verify it passes (signup 2025 gets full year data).
        var earlySignup = new DateOnly(2025, 11, 1);
        var slides = CreateStandardSlides();
        var wrapped = AnnualWrapped.Generate(2026, slides, _dec20, earlySignup);
        wrapped.Year.ShouldBe(2026);
    }


}
