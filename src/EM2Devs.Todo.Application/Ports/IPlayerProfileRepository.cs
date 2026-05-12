using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;

namespace EM2Devs.Todo.Application.Ports;

public interface IPlayerProfileRepository
{
    Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default);
    Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, DateOnly? historyDate = null, string? historySource = null, CancellationToken ct = default);
    Task RecordCompletionAsync(DateOnly completionDate, CancellationToken ct = default);
    Task ProcessDayEndAsync(DateOnly evaluationDate, CancellationToken ct = default);

    /// <summary>
    /// Activates a streak freeze for the current user's profile starting today.
    /// Throws <see cref="Domain.Exceptions.DomainException"/> if already frozen.
    /// </summary>
    Task FreezeStreakAsync(DateOnly today, int days, CancellationToken ct = default);
    Task StartFocusModeAsync(TaskId taskId, DateTimeOffset startedAt, CancellationToken ct = default);
    Task<FocusMode> EndFocusModeAsync(DateTimeOffset endedAt, CancellationToken ct = default);
    Task DiscoverSkillTreeAsync(SkillTreeType type, CancellationToken ct = default);
    Task AwardTitleAsync(Title title, CancellationToken ct = default);

    /// <summary>
    /// Wholesale-replaces the current user's profile with the supplied reconstructed
    /// aggregate. Used by data import to restore progression state from a snapshot.
    /// </summary>
    Task ImportAsync(PlayerProfile profile, CancellationToken ct = default);
}
