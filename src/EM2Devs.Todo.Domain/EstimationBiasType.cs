namespace EM2Devs.Todo.Domain;

/// <summary>
/// Represents the type of estimation bias detected for a task category.
/// </summary>
public enum EstimationBiasType
{
    /// <summary>No significant bias detected — estimates are accurate within threshold.</summary>
    None,

    /// <summary>User consistently underestimates — tasks take longer than estimated.</summary>
    Underestimation,

    /// <summary>User consistently overestimates — tasks take less time than estimated.</summary>
    Overestimation,

    /// <summary>User dramatically overestimates (>100% average) — requires immediate review.</summary>
    DramaticOverestimation,
}
