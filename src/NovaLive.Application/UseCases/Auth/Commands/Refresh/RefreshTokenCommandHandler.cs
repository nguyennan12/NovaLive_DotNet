using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler(IAppDbContext db, IAuthPersistence persistence,
    IRefreshTokenService tokens, IDateTimeProvider clock, AuthSessionService sessions,
    ILogger<RefreshTokenCommandHandler> logger) : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = tokens.Hash(request.Data.RefreshToken);
        var userId = await db.RefreshTokens.Where(token => token.TokenHash == hash)
            .Select(token => (Guid?)token.UserId).SingleOrDefaultAsync(ct);
        if (userId is null) return AuthErrors.InvalidRefresh;
        await persistence.LockUserAsync(userId.Value, ct);
        var token = await db.RefreshTokens.AsNoTracking().SingleAsync(token => token.TokenHash == hash, ct);
        var now = clock.UtcNow;
        if (token.RevokedAt != null)
        {
            await persistence.RevokeAllAsync(token.UserId, now, ct);
            logger.LogWarning("Refresh token reuse detected; all refresh sessions revoked for user {UserId}.", token.UserId);
            return AuthErrors.InvalidRefresh;
        }
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(user => user.Id == token.UserId, ct);
        if (user is null || user.AccountStatus != AccountStatus.Active || user.DeletedAt != null)
        {
            await persistence.RevokeAllAsync(token.UserId, now, ct);
            return AuthErrors.InvalidRefresh;
        }
        if (token.ExpiresAt <= now) return AuthErrors.InvalidRefresh;
        if (!await persistence.ConsumeRefreshTokenAsync(token.Id, now, ct))
        {
            await persistence.RevokeAllAsync(token.UserId, now, ct);
            return AuthErrors.InvalidRefresh;
        }
        return await sessions.IssueAsync(user, request.IpAddress, request.UserAgent, ct);
    }
}
