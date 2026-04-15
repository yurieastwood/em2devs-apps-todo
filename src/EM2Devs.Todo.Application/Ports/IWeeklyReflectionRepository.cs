using EM2Devs.Todo.Application.ReadModels;

namespace EM2Devs.Todo.Application.Ports;

/// <summary>
/// Persists the user's free-text weekly review reflection (what went well,
/// what dragged, adjustment). Scoped to the current authenticated user and
/// keyed by the Sunday anchoring the review week. Returns <c>null</c> when
/// the user has not yet saved a reflection for the given week.
/// </summary>
public interface IWeeklyReflectionRepository
{
    /// <summary>
    /// Fetches the saved reflection for the given week, scoped to the current user.
    /// </summary>
    /// <param name="weekOf">The Sunday anchoring the review week.</param>
    Task<WeeklyReflectionReadModel?> GetAsync(DateOnly weekOf, CancellationToken ct = default);

    /// <summary>
    /// Persists (insert or replace) the reflection for the given week, scoped
    /// to the current user.
    /// </summary>
    Task SaveAsync(
        DateOnly weekOf,
        string whatWentWell,
        string whatDragged,
        string adjustment,
        DateTimeOffset savedAt,
        CancellationToken ct = default);
}
