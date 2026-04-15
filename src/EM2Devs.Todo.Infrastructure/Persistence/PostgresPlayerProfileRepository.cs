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
    private readonly IXpHistoryCache _xpHistoryCache;

    public PostgresPlayerProfileRepository(
        TodoDbContext dbContext,
        ILastXpBreakdownCache breakdownCache,
        IXpHistoryCache xpHistoryCache)
    {
        _dbContext = dbContext;
        _breakdownCache = breakdownCache;
        _xpHistoryCache = xpHistoryCache;
    }

    public async Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateAsync(ct).ConfigureAwait(false);
        return PlayerProfileProjection.Project(
            profile, _breakdownCache.GetCurrent(), _xpHistoryCache.GetAll());
    }

    public async Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, DateOnly? historyDate = null, string? historySource = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        PlayerProfile profile = await GetOrCreateAsync(ct).ConfigureAwait(false);
        profile.AwardXp(xp);
        _breakdownCache.SetCurrent(breakdown);

        if (historyDate is not null && !string.IsNullOrWhiteSpace(historySource))
        {
            profile.RecordXpEarning(historyDate.Value, xp, historySource);
            _xpHistoryCache.Append(historyDate.Value, xp.Value, historySource);
        }

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
        // Single-user demo mode: one singleton row identified by PlayerProfile.SingletonId.
        PlayerProfile? profile = await _dbContext.PlayerProfiles
            .FirstOrDefaultAsync(p => p.Id == PlayerProfile.SingletonId, ct)
            .ConfigureAwait(false);
        if (profile is not null)
        {
            return profile;
        }

        // Race: two concurrent first-request handlers both reach here after each saw null.
        // Both Add() the same SingletonId, both SaveChanges(); Postgres' primary-key
        // constraint guarantees exactly one succeeds, and the loser catches DbUpdateException.
        // On the loser path, clear the tracker (the failed Add() left the entity in Added
        // state) and re-read — the winner's row is now visible.
        profile = PlayerProfile.NewProfile();
        _dbContext.PlayerProfiles.Add(profile);
        try
        {
            await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return profile;
        }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            PlayerProfile? existing = await _dbContext.PlayerProfiles
                .FirstOrDefaultAsync(p => p.Id == PlayerProfile.SingletonId, ct)
                .ConfigureAwait(false);
            if (existing is not null)
            {
                return existing;
            }

            throw;
        }
    }
}
