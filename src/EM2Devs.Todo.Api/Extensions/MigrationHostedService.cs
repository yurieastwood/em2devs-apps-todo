using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Api.Extensions;

internal sealed partial class MigrationHostedService(
    IServiceProvider serviceProvider,
    ILogger<MigrationHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        LogApplyingMigrations(logger);
        await dbContext.Database.MigrateAsync(stoppingToken).ConfigureAwait(false);
        LogMigrationsApplied(logger);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying database migrations...")]
    private static partial void LogApplyingMigrations(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database migrations applied successfully.")]
    private static partial void LogMigrationsApplied(ILogger logger);
}
