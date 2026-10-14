using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
namespace NovaLive.Infrastructure.Tests.Carts;
public sealed class CartSaveFailureInterceptor(string constraint) : SaveChangesInterceptor
{
    public bool Enabled { get; set; }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (!Enabled) return ValueTask.FromResult(result);
        Enabled = false;
        throw new DbUpdateException("Simulated unique race",
            new PostgresException("Duplicate", "ERROR", "ERROR", "23505", constraintName: constraint));
    }
}
