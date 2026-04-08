using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// EF Core-backed repository for the singleton PlayerProfile aggregate.
/// Single-user demo mode: returns or creates a single profile row.
/// When auth lands, this becomes per-UserId.
/// </summary>
public sealed class PostgresPlayerProfileRepository : IPlayerProfileRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ILastXpBreakdownCache _breakdownCache;

    public PostgresPlayerProfileRepository(TodoDbContext dbContext, ILastXpBreakdownCache breakdownCache)
    {
        _dbContext = dbContext;
        _breakdownCache = breakdownCache;
    }

    public async Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateAsync(ct).ConfigureAwait(false);

        return new PlayerProfileReadModel(
            TotalXp: profile.Level.CurrentXp.Value,
            Level: profile.Level.Value,
            XpToNextLevel: profile.Level.XpToNextLevel(),
            CurrentStreak: profile.Streak.CurrentDays,
            LongestStreak: profile.LongestStreak,
            LastXpBreakdown: _breakdownCache.GetCurrent());
    }

    public async Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        PlayerProfile profile = await GetOrCreateAsync(ct).ConfigureAwait(false);
        profile.AwardXp(xp);
        _breakdownCache.SetCurrent(breakdown);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RecordCompletionAsync(DateOnly completionDate, CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateAsync(ct).ConfigureAwait(false);
        profile.RecordCompletion(completionDate);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task ProcessDayEndAsync(DateOnly evaluationDate, CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateAsync(ct).ConfigureAwait(false);
        profile.ProcessDayEnd(evaluationDate);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task<PlayerProfile> GetOrCreateAsync(CancellationToken ct)
    {
        // Single-user demo mode: there is only ever one PlayerProfile row.
        // No OrderBy needed (the strongly-typed Id makes ordering by Id.Value untranslatable).
        PlayerProfile? profile = await _dbContext.PlayerProfiles
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        if (profile is null)
        {
            profile = PlayerProfile.NewProfile();
            _dbContext.PlayerProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        }

        return profile;
    }
}
