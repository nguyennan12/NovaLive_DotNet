using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Clock;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
namespace NovaLive.Application.Auth;

public sealed class LogoutCommandHandler(IAppDbContext db, IAuthPersistence persistence, ICurrentUser currentUser,
    IRefreshTokenService tokens, IDateTimeProvider clock, AuthSessionService sessions) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is not Guid userId) return AuthErrors.InvalidCredentials;
        await persistence.LockUserAsync(userId, ct);
        var hash = tokens.Hash(request.Data.RefreshToken);
        var token = await db.RefreshTokens.SingleOrDefaultAsync(item => item.TokenHash == hash && item.UserId == userId, ct);
        token?.Revoke(clock.UtcNow);
        await sessions.BlacklistCurrentAsync(ct);
        return Result.Success();
    }
}
public sealed class GetCurrentUserQueryHandler(IAppDbContext db, ICurrentUser currentUser, IPermissionProvider permissions,
    AuthSessionService sessions) : IQueryHandler<GetCurrentUserQuery, UserInfoResponse>
{
    public async Task<Result<UserInfoResponse>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(item => item.Id == currentUser.UserId, ct);
        if (user is null || user.AccountStatus != AccountStatus.Active || user.DeletedAt != null) return AuthErrors.InvalidCredentials;
        var (roles, shopId) = await sessions.GetContextAsync(user.Id, ct);
        return new UserInfoResponse(user.Id, user.Email, user.FullName, roles, shopId,
            await permissions.GetPermissionsAsync(roles, ct));
    }
}

