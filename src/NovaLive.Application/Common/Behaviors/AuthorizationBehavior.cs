using MediatR;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using System.Reflection;

namespace NovaLive.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUser currentUser,
    ICacheService cache,
    IUserRepository userRepository,
    IPermissionRepository permissionRepository)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private static readonly TimeSpan PermissionCacheDuration = TimeSpan.FromMinutes(5);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var type = request.GetType();
        var requiredPermissions = type.GetCustomAttributes<RequirePermissionAttribute>()
            .SelectMany(attribute => attribute.Permissions).ToList();

        if (request is IRequirePermission permissionRequest)
            requiredPermissions.Add(permissionRequest.RequiredPermission);

        var roleAttributes = type.GetCustomAttributes<AuthorizeRoleAttribute>().ToArray();

        if (requiredPermissions.Count == 0 && roleAttributes.Length == 0 && !type.IsDefined(typeof(RequireAuthenticatedAttribute)))
        {
            return await next(cancellationToken);
        }

        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return CreateFailure<TResponse>(
                new Error(ErrorType.Unauthorized, "Authorization.Unauthorized", "Vui lòng đăng nhập để thực hiện thao tác này."));
        }

        if (roleAttributes.Any(attribute => !attribute.Roles.Any(role => currentUser.Roles.Contains(role, StringComparer.Ordinal))))
        {
            return CreateFailure<TResponse>(
                new Error(ErrorType.Forbidden, "Authorization.Forbidden", "Bạn không có quyền thực hiện thao tác này."));
        }

        if (requiredPermissions.Count > 0)
        {
            if (currentUser.Roles.Count == 0)
            {
                return CreateFailure<TResponse>(
                    new Error(ErrorType.Forbidden, "Authorization.Forbidden", "Bạn không có quyền thực hiện thao tác này."));
            }

            var userId = currentUser.UserId.Value;
            var roleKey = string.Join(",", currentUser.Roles.OrderBy(r => r));
            var cacheKey = $"perm:{userId}:{roleKey}";
            var granted = await cache.GetAsync<string[]>(cacheKey, cancellationToken);

            if (granted is null)
            {
                var user = await userRepository.GetByIdAsync(userId, cancellationToken);

                if (user is null || user.AccountStatus != AccountStatus.Active || user.DeletedAt != null)
                {
                    return CreateFailure<TResponse>(
                        new Error(ErrorType.Unauthorized, "Authorization.Unauthorized", "Tài khoản không tồn tại hoặc đã bị khóa."));
                }

                granted = await permissionRepository.GetPermissionCodesAsync(userId, currentUser.Roles, cancellationToken);
                await cache.SetAsync(cacheKey, granted, PermissionCacheDuration, cancellationToken);
            }

            if (requiredPermissions.Any(permission => !granted.Contains(permission, StringComparer.Ordinal)))
            {
                return CreateFailure<TResponse>(
                    new Error(ErrorType.Forbidden, "Authorization.Forbidden", "Bạn không có quyền thực hiện thao tác này."));
            }
        }

        return await next(cancellationToken);
    }

    private static TResult CreateFailure<TResult>(Error error)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(error) as TResult)!;
        }

        var resultType = typeof(TResult).GenericTypeArguments[0];
        var failure = typeof(Result<>)
            .MakeGenericType(resultType)
            .GetMethod(nameof(Result<object>.Failure))!
            .Invoke(null, [error])!;

        return (TResult)failure;
    }
}
