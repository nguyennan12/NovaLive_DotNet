using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.UseCases.Auth.Errors;

namespace NovaLive.Application.UseCases.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    IUserRepository users,
    IPermissionRepository permissions,
    ICurrentUser currentUser,
    ICacheService cache,
    AuthSessionService sessions) : IQueryHandler<GetCurrentUserQuery, UserInfoResponse>
{
    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<Result<UserInfoResponse>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        if (!currentUser.UserId.HasValue) return AuthErrors.InvalidCredentials;

        var user = await users.GetByIdAsync(currentUser.UserId.Value, ct);
        if (user is null || user.AccountStatus != AccountStatus.Active || user.DeletedAt != null)
            return AuthErrors.InvalidCredentials;

        var (roles, shopId) = await sessions.GetContextAsync(user.Id, ct);

        var roleKey = string.Join(",", roles.OrderBy(r => r));
        var cacheKey = $"perm:{user.Id}:{roleKey}";
        var perms = await cache.GetAsync<string[]>(cacheKey, ct);
        if (perms is null)
        {
            perms = await permissions.GetPermissionCodesAsync(user.Id, roles, ct);
            await cache.SetAsync(cacheKey, perms, PermissionCacheDuration, ct);
        }

        return new UserInfoResponse(user.Id, user.Email, user.FullName, roles, shopId, perms);
    }
}
