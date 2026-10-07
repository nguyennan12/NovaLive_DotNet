namespace NovaLive.Application.Abstractions.Persistence;

public interface IMigrationService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
