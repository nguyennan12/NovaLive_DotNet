using System.Security.Claims;
using NovaLive.Application.Abstractions.Auth;

namespace NovaLive.Api.Auth;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public Guid? ShopId
    {
        get
        {
            var value = User?.FindFirstValue("shopId") ?? User?.FindFirstValue("shop_id");
            return Guid.TryParse(value, out var shopId) ? shopId : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray()
        ?? [];

    public IReadOnlyCollection<string> Permissions =>
        User?.FindAll("permission").Select(claim => claim.Value).ToArray()
        ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase) ||
        Roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

    public bool IsInRole(string role) =>
        User?.IsInRole(role) ?? false;
}
