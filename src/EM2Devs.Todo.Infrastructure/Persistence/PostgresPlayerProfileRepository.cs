using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

/// <summary>
/// EF Core-backed repository for the PlayerProfile aggregate.
/// Slice 3 multi-user isolation: every read/write scopes to <see cref="ICurrentUser.UserId"/>
/// and creates a fresh profile on first access for that user. The unique index on
/// <c>user_id</c> arbitrates concurrent create-on-first-request races.
/// </summary>
public sealed class PostgresPlayerProfileRepository : IPlayerProfileRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ILastXpBreakdownCache _breakdownCache;
    private readonly ICurrentUser _currentUser;

    public PostgresPlayerProfileRepository(
        TodoDbContext dbContext,
        ILastXpBreakdownCache breakdownCache,
        ICurrentUser currentUser)
    {
        ArgumentNullException.ThrowIfNull(currentUser);
        _dbContext = dbContext;
        _breakdownCache = breakdownCache;
        _currentUser = currentUser;
    }

    public async Task<PlayerProfileReadModel> GetProfileAsync(CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateForCurrentUserAsync(ct).ConfigureAwait(false);
        return PlayerProfileProjection.Project(profile, _breakdownCache.GetCurrent());
    }

    public async Task AwardXpAsync(ExperiencePoints xp, XpBreakdownReadModel? breakdown = null, DateOnly? historyDate = null, string? historySource = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(xp);

        PlayerProfile profile = await GetOrCreateForCurrentUserAsync(ct).ConfigureAwait(false);
        profile.AwardXp(xp);
        _breakdownCache.SetCurrent(breakdown);

        if (historyDate is not null && !string.IsNullOrWhiteSpace(historySource))
        {
            profile.RecordXpEarning(historyDate.Value, xp, historySource);
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task RecordCompletionAsync(DateOnly completionDate, CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateForCurrentUserAsync(ct).ConfigureAwait(false);
        profile.RecordCompletion(completionDate);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task ProcessDayEndAsync(DateOnly evaluationDate, CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateForCurrentUserAsync(ct).ConfigureAwait(false);
        profile.ProcessDayEnd(evaluationDate);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task FreezeStreakAsync(DateOnly today, int days, CancellationToken ct = default)
    {
        PlayerProfile profile = await GetOrCreateForCurrentUserAsync(ct).ConfigureAwait(false);
        profile.FreezeStreak(today, days);
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task<PlayerProfile> GetOrCreateForCurrentUserAsync(CancellationToken ct)
    {
        Guid userId = _currentUser.UserId;

        // Scope every lookup to the current user's profile.
        PlayerProfile? profile = await _dbContext.PlayerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            .ConfigureAwait(false);
        if (profile is not null)
        {
            return profile;
        }

        // Race: two concurrent first-request handlers for the same user both reach here
        // after each saw null. Both Add() a new PlayerProfile for the same UserId, both
        // SaveChanges(); the unique index on user_id guarantees exactly one succeeds,
        // and the loser catches DbUpdateException. On the loser path, clear the tracker
        // (the failed Add() left the entity in Added state) and re-read — the winner's
        // row is now visible.
        profile = PlayerProfile.NewProfile(userId);
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
                .FirstOrDefaultAsync(p => p.UserId == userId, ct)
                .ConfigureAwait(false);
            if (existing is not null)
            {
                return existing;
            }

            throw;
        }
    }
}
