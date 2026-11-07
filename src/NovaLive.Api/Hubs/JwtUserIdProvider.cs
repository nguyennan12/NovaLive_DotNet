using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace NovaLive.Api.Hubs;

// JWT inbound claim mapping is disabled; SignalR's default NameIdentifier lookup misses "sub".
public sealed class JwtUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var value = connection.User.FindFirstValue("sub") ?? connection.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) && id != Guid.Empty ? id.ToString() : null;
    }
}
