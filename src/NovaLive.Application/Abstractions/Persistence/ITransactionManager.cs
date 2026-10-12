using NovaLive.Domain.Common;
namespace NovaLive.Application.Abstractions.Persistence;

public interface ITransactionManager
{
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, bool commitOnFailure, CancellationToken cancellationToken);
}
public interface IAuthPersistence
{
    Task LockUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<Error?> SaveRegistrationAsync(CancellationToken cancellationToken);
    Task<bool> ConsumeRefreshTokenAsync(Guid tokenId, DateTimeOffset now, CancellationToken cancellationToken);
    Task RevokeAllAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);
}
public interface IAfterCommitActions
{
    void Add(Func<CancellationToken, Task> action);
    Task RunAsync(CancellationToken cancellationToken);
    void Clear();
}

