using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Queries.GetCurrentUser;

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
