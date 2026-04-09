using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Api.Extensions;

internal static partial class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TodoDbContext>>();

        LogApplyingMigrations(logger);
        await dbContext.Database.MigrateAsync().ConfigureAwait(false);
        LogMigrationsApplied(logger);
    }

    /// <summary>
    /// Ensures the singleton PlayerProfile row exists before any HTTP request arrives.
    /// Without this, concurrent first-request handlers race inside
    /// <c>PostgresPlayerProfileRepository.GetOrCreateAsync</c> and can each create their
    /// own profile row, leaving the table with two rows and non-deterministic reads.
    /// Called from startup after migrations; the profile is single-user demo mode, so one
    /// row is the correct steady state.
    /// </summary>
    public static async Task SeedPlayerProfileAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TodoDbContext>>();

        bool exists = await dbContext.PlayerProfiles.AnyAsync().ConfigureAwait(false);
        if (exists)
        {
            LogPlayerProfileAlreadySeeded(logger);
            return;
        }

        dbContext.PlayerProfiles.Add(PlayerProfile.NewProfile());
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
        LogPlayerProfileSeeded(logger);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying database migrations...")]
    private static partial void LogApplyingMigrations(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database migrations applied successfully.")]
    private static partial void LogMigrationsApplied(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Singleton PlayerProfile already seeded.")]
    private static partial void LogPlayerProfileAlreadySeeded(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded singleton PlayerProfile row.")]
    private static partial void LogPlayerProfileSeeded(ILogger logger);
}
