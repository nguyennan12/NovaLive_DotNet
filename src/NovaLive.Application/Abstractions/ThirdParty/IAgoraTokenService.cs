namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IAgoraTokenService
{
    string GeneratePublisherToken(string channelName, string userId, TimeSpan? ttl = null);
    string GenerateSubscriberToken(string channelName, string userId, TimeSpan? ttl = null);
    string GenerateToken(string channelName, string userId, AgoraRtcRole role, TimeSpan? ttl = null);
}
