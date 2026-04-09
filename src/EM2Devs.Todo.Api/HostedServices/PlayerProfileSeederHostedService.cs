using EM2Devs.Todo.Domain.Entities;
using EM2Devs.Todo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EM2Devs.Todo.Api.HostedServices;

/// <summary>
/// Ensures the singleton <see cref="PlayerProfile"/> row exists before the host starts
/// accepting HTTP requests. Without this, two concurrent first-request handlers can race
/// inside <c>PostgresPlayerProfileRepository.GetOrCreateAsync</c> (both see no row, both
/// insert) and leave the table with two rows — a silent bug that corrupts subsequent
/// progression reads.
///
/// Implemented as <see cref="IHostedService"/> (rather than an inline call in
/// <c>Program.cs</c>) so that <c>dotnet ef database update</c> — which builds the DI
/// container to discover the <c>DbContext</c> but does not invoke <c>host.Run()</c> —
/// does not trigger the seed against a database whose schema has not yet been migrated.
/// </summary>
internal sealed partial class PlayerProfileSeederHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PlayerProfileSeederHostedService> _logger;

    public PlayerProfileSeederHostedService(
        IServiceProvider services,
        ILogger<PlayerProfileSeederHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _services.CreateScope();
        TodoDbContext dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        bool exists = await dbContext.PlayerProfiles
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);

        if (exists)
        {
            LogAlreadySeeded(_logger);
            return;
        }

        dbContext.PlayerProfiles.Add(PlayerProfile.NewProfile());
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        LogSeeded(_logger);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Singleton PlayerProfile already seeded; skipping.")]
    private static partial void LogAlreadySeeded(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded singleton PlayerProfile row.")]
    private static partial void LogSeeded(ILogger logger);
}
