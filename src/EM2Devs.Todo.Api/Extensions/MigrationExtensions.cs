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

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying database migrations...")]
    private static partial void LogApplyingMigrations(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database migrations applied successfully.")]
    private static partial void LogMigrationsApplied(ILogger logger);
}
