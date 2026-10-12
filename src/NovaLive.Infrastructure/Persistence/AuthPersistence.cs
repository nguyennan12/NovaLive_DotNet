using Microsoft.EntityFrameworkCore;
using Npgsql;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Domain.Common;
namespace NovaLive.Infrastructure.Persistence;

public sealed class AuthPersistence(AppDbContext db) : IAuthPersistence
{
    public async Task LockUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (db.Database.IsNpgsql())
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({userId.ToString()}, 808));", cancellationToken);
    }
    public async Task<Error?> SaveRegistrationAsync(CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); return null; }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ix_users_email" or "ix_users_phone" })
        {
            db.ChangeTracker.Clear();
            return Error.Conflict("Auth.DuplicateAccount", "Email or phone is already registered.");
        }
    }
    public async Task<bool> ConsumeRefreshTokenAsync(Guid tokenId, DateTimeOffset now, CancellationToken cancellationToken) =>
        await db.RefreshTokens.Where(token => token.Id == tokenId && token.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAt, now), cancellationToken) == 1;
    public Task RevokeAllAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken) =>
        db.RefreshTokens.Where(token => token.UserId == userId && token.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(token => token.RevokedAt, now), cancellationToken);
}
