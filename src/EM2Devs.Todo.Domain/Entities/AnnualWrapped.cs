using EM2Devs.Todo.Domain.Exceptions;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Domain.Entities;

/// <summary>
/// An annual productivity summary presented as an interactive slideshow.
/// Maps to: docs/features/reflection/annual-wrapped.feature
/// </summary>
public sealed class AnnualWrapped
{
    private const int MinimumMonthsRequired = 3;
    private const int WrappedAvailableDay = 15;
    private const int WrappedAvailableMonth = 12;

    private readonly List<WrappedSlide> _slides;

    public AnnualWrappedId Id { get; }
    public int Year { get; }
    public IReadOnlyList<WrappedSlide> Slides => _slides.AsReadOnly();
    public bool IsPartialYear { get; }
    public DateOnly? DataStartDate { get; }
    public int CurrentSlideIndex { get; private set; }
    public bool HasBranding { get; }

    private AnnualWrapped(
        AnnualWrappedId id,
        int year,
        List<WrappedSlide> slides,
        bool isPartialYear,
        DateOnly? dataStartDate)
    {
        Id = id;
        Year = year;
        _slides = slides;
        IsPartialYear = isPartialYear;
        DataStartDate = dataStartDate;
        HasBranding = true;
    }

    /// <summary>
    /// Generates the annual wrapped for the given year.
    /// Must be December 15 or later, and the user must have at least 3 months of data.
    /// </summary>
    public static AnnualWrapped Generate(
        int year,
        IReadOnlyList<WrappedSlide> slides,
        DateOnly today,
        DateOnly signupDate)
    {
        ArgumentNullException.ThrowIfNull(slides);

        if (slides.Count == 0)
        {
            throw new DomainException("Annual wrapped must have at least one slide.");
        }

        if (today.Year == year && (today.Month < WrappedAvailableMonth || today.Day < WrappedAvailableDay))
        {
            throw new DomainException("Annual wrapped is only available after December 15.");
        }

        int monthsOfData = CalculateMonthsOfData(signupDate, today, year);
        if (monthsOfData < MinimumMonthsRequired)
        {
            throw new DomainException(
                $"Insufficient data for annual wrapped. At least {MinimumMonthsRequired} months required, but only {monthsOfData} months of data available.");
        }

        bool isPartialYear = signupDate.Year == year && signupDate.Month > 1;
        DateOnly? dataStart = isPartialYear ? signupDate : null;

        return new AnnualWrapped(
            AnnualWrappedId.New(), year, new List<WrappedSlide>(slides), isPartialYear, dataStart);
    }

    /// <summary>
    /// Loads a historical wrapped from persisted data.
    /// </summary>
    public static AnnualWrapped LoadHistorical(
        int year,
        IReadOnlyList<WrappedSlide> slides,
        bool isPartialYear,
        DateOnly? dataStartDate)
    {
        ArgumentNullException.ThrowIfNull(slides);

        if (slides.Count == 0)
        {
            throw new DomainException("Annual wrapped must have at least one slide.");
        }

        return new AnnualWrapped(
            AnnualWrappedId.New(), year, new List<WrappedSlide>(slides), isPartialYear, dataStartDate);
    }

    /// <summary>
    /// Navigate forward through the slideshow.
    /// </summary>
    public void NavigateForward()
    {
        if (CurrentSlideIndex >= _slides.Count - 1)
        {
            throw new DomainException("Already at the last slide.");
        }

        CurrentSlideIndex++;
    }

    /// <summary>
    /// Navigate backward through the slideshow.
    /// </summary>
    public void NavigateBackward()
    {
        if (CurrentSlideIndex <= 0)
        {
            throw new DomainException("Already at the first slide.");
        }

        CurrentSlideIndex--;
    }

    /// <summary>
    /// Gets the current slide in the slideshow.
    /// </summary>
    public WrappedSlide GetCurrentSlide() => _slides[CurrentSlideIndex];

    /// <summary>
    /// Generates a shareable version of a specific slide, including Waypoint branding.
    /// Excludes data the user has marked as excluded.
    /// </summary>
    public WrappedSlide GetShareableSlide(int slideIndex)
    {
        if (slideIndex < 0 || slideIndex >= _slides.Count)
        {
            throw new DomainException($"Slide index {slideIndex} is out of range.");
        }

        var slide = _slides[slideIndex];
        if (slide.IsExcludedFromShare)
        {
            throw new DomainException("Cannot share a slide that has been excluded from sharing.");
        }

        return slide.EnableSharing();
    }

    /// <summary>
    /// Excludes a specific slide from the shareable wrapped.
    /// The slide remains visible in the private view.
    /// </summary>
    public void ExcludeSlideFromShare(int slideIndex)
    {
        if (slideIndex < 0 || slideIndex >= _slides.Count)
        {
            throw new DomainException($"Slide index {slideIndex} is out of range.");
        }

        _slides[slideIndex] = _slides[slideIndex].ExcludeFromShare();
    }

    /// <summary>
    /// Includes a previously excluded slide back into the shareable wrapped.
    /// </summary>
    public void IncludeSlideInShare(int slideIndex)
    {
        if (slideIndex < 0 || slideIndex >= _slides.Count)
        {
            throw new DomainException($"Slide index {slideIndex} is out of range.");
        }

        _slides[slideIndex] = _slides[slideIndex].IncludeInShare();
    }

    /// <summary>
    /// Checks whether the wrapped is available for viewing, returning an availability message.
    /// </summary>
    public static (bool IsAvailable, string Message) CheckAvailability(
        DateOnly today, DateOnly signupDate, int year)
    {
        if (today.Year == year && (today.Month < WrappedAvailableMonth || today.Day < WrappedAvailableDay))
        {
            return (false, "Your wrapped will be available after December 15.");
        }

        int monthsOfData = CalculateMonthsOfData(signupDate, today, year);
        if (monthsOfData < MinimumMonthsRequired)
        {
            return (false, "Your wrapped will be available next year. Here is a teaser of what wrapped will include: total tasks, XP earned, levels gained, and more.");
        }

        return (true, "Your wrapped is ready!");
    }

    private static int CalculateMonthsOfData(DateOnly signupDate, DateOnly today, int year)
    {
        if (signupDate.Year > year)
        {
            return 0;
        }

        int startMonth = signupDate.Year < year ? 1 : signupDate.Month;
        int endMonth = 12;

        return endMonth - startMonth;
    }
}
