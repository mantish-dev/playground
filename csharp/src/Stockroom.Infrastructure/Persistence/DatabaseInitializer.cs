using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Stockroom.Infrastructure.Persistence;

/// <summary>
/// Creates the schema on startup. A migration pipeline replaces this once the schema needs to evolve
/// in place; for a single-file SQLite store, EnsureCreated keeps first-run friction at zero.
/// </summary>
internal sealed class DatabaseInitializer(IServiceScopeFactory scopeFactory, ILogger<DatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StockroomDbContext>();

        var created = await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        if (created)
        {
            logger.SchemaCreated();
        }
        else
        {
            logger.SchemaAlreadyPresent();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
