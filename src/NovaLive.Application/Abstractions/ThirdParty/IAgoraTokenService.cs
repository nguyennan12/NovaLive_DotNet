namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IAgoraTokenService
{
    string CreateRtcToken(string channelName, Guid userId, string role, TimeSpan ttl);
}
