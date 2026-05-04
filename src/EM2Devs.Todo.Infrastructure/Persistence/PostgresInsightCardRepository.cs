using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Domain;
using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EM2Devs.Todo.Infrastructure.Persistence;

// TODO(ADR-029): candidate for Dapper migration — read-model trio per ADR-009.
public sealed class PostgresInsightCardRepository : IInsightCardRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresInsightCardRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InsightCard>> GetForCurrentUserAsync(bool includeRead, CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        IQueryable<InsightCard> query = _dbContext.InsightCards
            .Where(c => EF.Property<Guid>(c, "UserId") == userId
                     && c.Status != InsightCardStatus.Dismissed);

        if (!includeRead)
        {
            query = query.Where(c => c.Status == InsightCardStatus.Unread || c.Status == InsightCardStatus.Saved);
        }

        return await query.ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<InsightCard?> GetByIdAsync(InsightCardId id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        Guid userId = _currentUser.UserId;
        return await _dbContext.InsightCards
            .FirstOrDefaultAsync(c => c.Id == id && EF.Property<Guid>(c, "UserId") == userId, ct)
            .ConfigureAwait(false);
    }

    public async Task AddAsync(InsightCard card, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(card);
        EntityEntry<InsightCard> entry = _dbContext.InsightCards.Add(card);
        entry.Property("UserId").CurrentValue = _currentUser.UserId;
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task SaveAsync(InsightCard card, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(card);
        // Card was retrieved via this DbContext, so EF tracks its property mutations.
        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
