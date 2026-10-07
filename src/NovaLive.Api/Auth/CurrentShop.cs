using System.Security.Claims;
using NovaLive.Application.Abstractions.Auth;

namespace NovaLive.Api.Auth;

public sealed class CurrentShop(IHttpContextAccessor httpContextAccessor) : ICurrentShop
{
    public Guid? ShopId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return null;
            }

            // 1. Try reading from route/query values
            if (httpContext.Request.RouteValues.TryGetValue("shopId", out var routeValue) &&
                Guid.TryParse(routeValue?.ToString(), out var routeShopId))
            {
                return routeShopId;
            }

            // 2. Try reading from Request Headers
            if (httpContext.Request.Headers.TryGetValue("X-Shop-Id", out var headerValue) &&
                Guid.TryParse(headerValue.FirstOrDefault(), out var headerShopId))
            {
                return headerShopId;
            }

            // 3. Try reading from Claims in JWT
            var claimValue = httpContext.User.FindFirstValue("shopId") ?? httpContext.User.FindFirstValue("shop_id");
            return Guid.TryParse(claimValue, out var claimShopId) ? claimShopId : null;
        }
    }
}
