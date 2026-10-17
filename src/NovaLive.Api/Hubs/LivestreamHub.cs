using MediatR;
using Microsoft.AspNetCore.SignalR;
using NovaLive.Api.Services;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Livestreams.Commands.SendComment;
using NovaLive.Contracts.V1.Livestreams;

namespace NovaLive.Api.Hubs;

public sealed class LivestreamHub(
    ISender sender,
    ILivestreamCacheManager cacheManager,
    ILivestreamNotificationService notificationService,
    ILivestreamReactionBatcher reactionBatcher)
    : Hub
{
    private const string SessionIdKey = "SessionId";

    public async Task<int> JoinLiveSession(Guid sessionId)
    {
        if (Context.Items.TryGetValue(SessionIdKey, out var prev) && prev is Guid existingSessionId)
        {
            if (existingSessionId == sessionId)
            {
                return await cacheManager.GetViewerCountAsync(sessionId);
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(existingSessionId));
            var oldViewerCount = await cacheManager.DecrementViewerCountAsync(existingSessionId);
            await notificationService.BroadcastViewerCountUpdatedAsync(existingSessionId, oldViewerCount);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(sessionId));
        Context.Items[SessionIdKey] = sessionId;

        var viewerCount = await cacheManager.IncrementViewerCountAsync(sessionId);
        await notificationService.BroadcastViewerCountUpdatedAsync(sessionId, viewerCount);

        return viewerCount;
    }

    public async Task<int> LeaveLiveSession(Guid sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(sessionId));

        if (Context.Items.TryGetValue(SessionIdKey, out var val) && val is Guid trackedSessionId && trackedSessionId == sessionId)
        {
            Context.Items.Remove(SessionIdKey);
            var viewerCount = await cacheManager.DecrementViewerCountAsync(sessionId);
            await notificationService.BroadcastViewerCountUpdatedAsync(sessionId, viewerCount);
            return viewerCount;
        }

        return await cacheManager.GetViewerCountAsync(sessionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue(SessionIdKey, out var val) && val is Guid sessionId)
        {
            Context.Items.Remove(SessionIdKey);
            var viewerCount = await cacheManager.DecrementViewerCountAsync(sessionId);
            await notificationService.BroadcastViewerCountUpdatedAsync(sessionId, viewerCount);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task<LivestreamCommentResponse> SendMessage(string content)
    {
        var sessionId = GetActiveSessionId();
        return SendMessageToSession(sessionId, content);
    }

    public async Task<LivestreamCommentResponse> SendMessageToSession(Guid sessionId, string content)
    {
        var (userId, displayName) = ResolveUserInfo();

        var result = await sender.Send(new SendLivestreamCommentCommand(
            sessionId,
            userId,
            displayName,
            content));

        if (result.IsFailure)
        {
            throw new HubException(result.Error.Description);
        }

        return result.Value!;
    }

    public void SendReaction(string type)
    {
        var sessionId = GetActiveSessionId();
        SendReactionToSession(sessionId, type);
    }

    public void SendReactionToSession(Guid sessionId, string type)
    {
        reactionBatcher.AddReaction(sessionId, type);
    }

    private Guid GetActiveSessionId()
    {
        if (Context.Items.TryGetValue(SessionIdKey, out var val) && val is Guid sessionId)
        {
            return sessionId;
        }

        throw new HubException("SessionId is required. Please call JoinLiveSession first.");
    }

    private (Guid? UserId, string DisplayName) ResolveUserInfo()
    {
        Guid? userId = null;
        if (Guid.TryParse(Context.UserIdentifier, out var uid))
        {
            userId = uid;
        }

        var nameClaim = Context.User?.FindFirst("name")?.Value
            ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(nameClaim))
        {
            var emailClaim = Context.User?.FindFirst("email")?.Value
                ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (!string.IsNullOrWhiteSpace(emailClaim))
            {
                nameClaim = emailClaim.Split('@')[0];
            }
        }

        var displayName = !string.IsNullOrWhiteSpace(nameClaim)
            ? nameClaim
            : (userId.HasValue ? "Người dùng" : "Khách");

        return (userId, displayName);
    }

    private static string GroupName(Guid sessionId) => $"live_{sessionId:N}";
}
