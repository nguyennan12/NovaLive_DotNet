using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Rbac;

namespace NovaLive.Infrastructure.Persistence.Interceptors;

// One instance per DbContext, shared by its SaveChanges and transaction interceptors.
internal sealed class PermissionCacheInvalidation(IPermissionCacheInvalidator invalidator)
{
    private bool candidate;
    private bool pending;
    private Guid? transactionId;

    public void BeforeSave(DbContext db)
    {
        var current = db.Database.CurrentTransaction?.TransactionId;
        // Disposing an uncommitted transaction may not raise TransactionRolledBack.
        if (transactionId != current) pending = false;
        transactionId = current;
        db.ChangeTracker.DetectChanges();
        candidate = db.ChangeTracker.Entries().Any(entry =>
            entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            && entry.Entity is Role or Permission or RolePermission);
        if (candidate && global::System.Transactions.Transaction.Current is not null)
            throw new InvalidOperationException("RBAC writes require an EF-managed transaction, not an ambient TransactionScope.");
    }

    public async Task SavedAsync(DbContext db)
    {
        pending |= candidate;
        candidate = false;
        if (db.Database.CurrentTransaction is null) await CommittedAsync();
    }

    public void SaveFailed() => candidate = false;
    public void RolledBack() { pending = false; candidate = false; transactionId = null; }

    public async Task CommittedAsync(Guid? committedTransactionId = null)
    {
        if (committedTransactionId != transactionId) return;
        var invalidate = pending;
        pending = false;
        transactionId = null;
        // The database has already committed; request cancellation must not skip invalidation.
        if (invalidate) await invalidator.InvalidateAllAsync(CancellationToken.None);
    }
}

internal sealed class PermissionSaveInterceptor(PermissionCacheInvalidation state) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    { state.BeforeSave(eventData.Context!); return result; }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    { state.BeforeSave(eventData.Context!); return ValueTask.FromResult(result); }
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    { state.SavedAsync(eventData.Context!).GetAwaiter().GetResult(); return result; }
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    { await state.SavedAsync(eventData.Context!); return result; }
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => state.SaveFailed();
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    { state.SaveFailed(); return Task.CompletedTask; }
    public override void SaveChangesCanceled(DbContextEventData eventData) => state.SaveFailed();
    public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
    { state.SaveFailed(); return Task.CompletedTask; }
}

internal sealed class PermissionTransactionInterceptor(PermissionCacheInvalidation state) : DbTransactionInterceptor
{
    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) =>
        state.CommittedAsync(eventData.TransactionId).GetAwaiter().GetResult();
    public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = default) => state.CommittedAsync(eventData.TransactionId);
    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) => state.RolledBack();
    public override Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    { state.RolledBack(); return Task.CompletedTask; }
}
