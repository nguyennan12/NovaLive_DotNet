using MediatR;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;
using System.Reflection;

namespace NovaLive.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser currentUser, IPermissionProvider permissionProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var type = request.GetType();
        var permissions = type.GetCustomAttributes<RequirePermissionAttribute>().SelectMany(attribute => attribute.Permissions).ToList();
        if (request is IRequirePermission permissionRequest) permissions.Add(permissionRequest.RequiredPermission);
        var roleAttributes = type.GetCustomAttributes<AuthorizeRoleAttribute>().ToArray();
        if (permissions.Count == 0 && roleAttributes.Length == 0 && !type.IsDefined(typeof(RequireAuthenticatedAttribute)))
        {
            return await next(cancellationToken);
        }

        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return CreateFailure<TResponse>(
                new Error(ErrorType.Unauthorized, "Authorization.Unauthorized", "Vui lòng đăng nhập để thực hiện thao tác này."));
        }

        var granted = permissions.Count == 0 ? [] :
            await permissionProvider.GetPermissionsAsync(currentUser.Roles, cancellationToken);
        if (permissions.Any(permission => !granted.Contains(permission, StringComparer.Ordinal))
            || roleAttributes.Any(attribute => !attribute.Roles.Any(role => currentUser.Roles.Contains(role, StringComparer.Ordinal))))
        {
            return CreateFailure<TResponse>(
                new Error(ErrorType.Forbidden, "Authorization.Forbidden", "Bạn không có quyền thực hiện thao tác này."));
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
