using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Infrastructure.Persistence.Seeding;

namespace NovaLive.Infrastructure.Persistence;

public class MigrationService(
    AppDbContext context,
    IEnumerable<IDataSeeder> seeders,
    ILogger<MigrationService> logger) : IMigrationService
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting database migration process...");
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migration completed successfully.");

            logger.LogInformation("Executing database seeders...");
            foreach (var seeder in seeders.OrderBy(s => s.Order))
            {
                await seeder.SeedAsync(cancellationToken);
            }
            logger.LogInformation("All seeders executed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database migration and seeding.");
            throw;
        }
    }
}
