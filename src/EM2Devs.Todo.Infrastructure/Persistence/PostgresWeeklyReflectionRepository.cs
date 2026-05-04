using EM2Devs.Todo.Application.Ports;
using EM2Devs.Todo.Application.ReadModels;
using EM2Devs.Todo.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Infrastructure.Persistence;

// TODO(ADR-029): candidate for Dapper migration — read-model trio per ADR-009.
public sealed class PostgresWeeklyReflectionRepository : IWeeklyReflectionRepository
{
    private readonly TodoDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public PostgresWeeklyReflectionRepository(TodoDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<WeeklyReflectionReadModel?> GetAsync(DateOnly weekOf, CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        WeeklyReflectionRow? row = await _dbContext.WeeklyReflections
            .FirstOrDefaultAsync(r => r.UserId == userId && r.WeekOf == weekOf, ct)
            .ConfigureAwait(false);
        if (row is null)
        {
            return null;
        }

        return new WeeklyReflectionReadModel(row.WhatWentWell, row.WhatDragged, row.Adjustment, row.SavedAt);
    }

    public async Task SaveAsync(
        DateOnly weekOf,
        string whatWentWell,
        string whatDragged,
        string adjustment,
        DateTimeOffset savedAt,
        CancellationToken ct = default)
    {
        Guid userId = _currentUser.UserId;
        WeeklyReflectionRow? existing = await _dbContext.WeeklyReflections
            .FirstOrDefaultAsync(r => r.UserId == userId && r.WeekOf == weekOf, ct)
            .ConfigureAwait(false);

        if (existing is null)
        {
            _dbContext.WeeklyReflections.Add(new WeeklyReflectionRow
            {
                UserId = userId,
                WeekOf = weekOf,
                WhatWentWell = whatWentWell,
                WhatDragged = whatDragged,
                Adjustment = adjustment,
                SavedAt = savedAt,
            });
        }
        else
        {
            existing.WhatWentWell = whatWentWell;
            existing.WhatDragged = whatDragged;
            existing.Adjustment = adjustment;
            existing.SavedAt = savedAt;
        }

        await _dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
