using System.Security.Claims;
using NovaLive.Application.Abstractions.CurrentUser;

namespace NovaLive.CoreApi.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public Guid? ShopId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue("shopId");
            return Guid.TryParse(value, out var shopId) ? shopId : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray()
        ?? [];
}
