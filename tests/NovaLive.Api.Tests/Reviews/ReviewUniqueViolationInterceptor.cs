using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovaLive.Domain.Reviews;
using Npgsql;

namespace NovaLive.Api.Tests.Reviews;

internal sealed class ReviewUniqueViolationInterceptor(string constraint) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context?.ChangeTracker.Entries<Review>().Any(e => e.State == EntityState.Added) == true)
            throw new DbUpdateException("Injected PostgreSQL UNIQUE collision",
                new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation,
                    constraintName: constraint));
        return ValueTask.FromResult(result);
    }
}
