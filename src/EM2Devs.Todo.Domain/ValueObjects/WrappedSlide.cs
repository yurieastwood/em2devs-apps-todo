using EM2Devs.Todo.Domain.Exceptions;

namespace EM2Devs.Todo.Domain.ValueObjects;

/// <summary>
/// A single slide in an annual wrapped presentation.
/// Maps to: docs/features/reflection/annual-wrapped.feature
/// </summary>
public sealed record WrappedSlide
{
    public string Title { get; }
    public string Metric { get; }
    public string VisualizationType { get; }
    public bool IsShareable { get; private init; }
    public bool IsExcludedFromShare { get; private init; }

    public WrappedSlide(string title, string metric, string visualizationType)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Wrapped slide title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(metric))
        {
            throw new DomainException("Wrapped slide metric cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(visualizationType))
        {
            throw new DomainException("Wrapped slide visualization type cannot be empty.");
        }

        Title = title;
        Metric = metric;
        VisualizationType = visualizationType;
        IsShareable = true;
        IsExcludedFromShare = false;
    }

    /// <summary>
    /// Creates a copy of this slide with sharing enabled.
    /// </summary>
    public WrappedSlide EnableSharing() => this with { IsShareable = true, IsExcludedFromShare = false };

    /// <summary>
    /// Creates a copy of this slide excluded from sharing but still visible in the private view.
    /// </summary>
    public WrappedSlide ExcludeFromShare() => this with { IsExcludedFromShare = true };

    /// <summary>
    /// Creates a copy of this slide included in sharing.
    /// </summary>
    public WrappedSlide IncludeInShare() => this with { IsExcludedFromShare = false };

    /// <summary>
    /// Creates an encouraging slide for when there is no data for this metric.
    /// </summary>
    public static WrappedSlide CreateEncouraging(string title, string encouragingMessage, string visualizationType)
    {
        return new WrappedSlide(title, encouragingMessage, visualizationType);
    }
}
