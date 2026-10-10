using NovaLive.Api.Auth;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Cache;
using NovaLive.Domain.Common;
namespace NovaLive.Api.Middleware;

public sealed class JwtRoleContextMiddleware(RequestDelegate next, ILogger<JwtRoleContextMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ICacheService cache, ICurrentUser currentUser)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (currentUser.UserId is null || !Guid.TryParse(currentUser.Jti, out _) || currentUser.AccessTokenExpiresAt is null)
            {
                await AuthProblem.WriteAsync(context, Error.Unauthorized("Auth.InvalidToken", "Invalid access token."));
                return;
            }
            try
            {
                if (await cache.ExistsAsync("Blacklist:" + currentUser.Jti, context.RequestAborted))
                {
                    await AuthProblem.WriteAsync(context, Error.Unauthorized("Auth.TokenRevoked", "Access token was revoked."));
                    return;
                }
            }
            catch (Exception exception) when (!context.RequestAborted.IsCancellationRequested)
            {
                logger.LogError("Token revocation check unavailable; denying authenticated request. Failure type {FailureType}.",
                    exception.GetType().Name);
                await AuthProblem.WriteAsync(context, Error.Unauthorized("Auth.RevocationCheckUnavailable", "Unable to validate this session."));
                return;
            }
        }
        // CurrentUser is scoped and reads this validated HttpContext.User; it never accepts request-body role claims.
        await next(context);
    }
}

