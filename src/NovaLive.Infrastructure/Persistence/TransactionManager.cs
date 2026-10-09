using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Domain.Common;
namespace NovaLive.Infrastructure.Persistence;

public sealed class TransactionManager(AppDbContext db, IAfterCommitActions afterCommit) : ITransactionManager
{
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, bool commitOnFailure, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action();
            if (result is Result { IsFailure: true } && !commitOnFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                db.ChangeTracker.Clear();
                return result;
            }
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            // Delivery errors are handled by the queued action; a failed email cannot undo registration.
            await afterCommit.RunAsync(cancellationToken);
            return result;
        }
        finally { afterCommit.Clear(); }
    }
}
public sealed class AfterCommitActions : IAfterCommitActions
{
    private readonly List<Func<CancellationToken, Task>> actions = [];
    public void Add(Func<CancellationToken, Task> action) => actions.Add(action);
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        foreach (var action in actions) await action(cancellationToken);
    }
    public void Clear() => actions.Clear();
}
