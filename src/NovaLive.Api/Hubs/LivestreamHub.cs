using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NovaLive.Api.Hubs;

public sealed class LivestreamHub : Hub
{
    public async Task JoinLiveSession(Guid sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(sessionId));
    }

    public async Task LeaveLiveSession(Guid sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(sessionId));
    }

    [Authorize]
    public async Task SendMessage(Guid sessionId, string content)
    {
        await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveMessage", new
        {
            sessionId,
            senderId = Context.UserIdentifier,
            content,
            sentAt = DateTimeOffset.UtcNow
        });
    }

    [Authorize]
    public async Task SendReaction(Guid sessionId, string type)
    {
        await Clients.Group(GroupName(sessionId)).SendAsync("ReceiveReaction", new
        {
            sessionId,
            senderId = Context.UserIdentifier,
            type,
            sentAt = DateTimeOffset.UtcNow
        });
    }

    private static string GroupName(Guid sessionId) => $"live_{sessionId:N}";
}
