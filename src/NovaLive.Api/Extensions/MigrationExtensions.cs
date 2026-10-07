using NovaLive.Application.Abstractions.Persistence;

namespace NovaLive.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
        await migrationService.ExecuteAsync(cancellationToken);
    }
}
