using MediatR;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequirePermission permissionRequest)
        {
            return await next(cancellationToken);
        }

        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return CreateFailure<TResponse>(
                new Error(ErrorType.Unauthorized, "Authorization.Unauthorized", "Vui lòng đăng nhập để thực hiện thao tác này."));
        }

        if (!currentUser.HasPermission(permissionRequest.RequiredPermission))
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
