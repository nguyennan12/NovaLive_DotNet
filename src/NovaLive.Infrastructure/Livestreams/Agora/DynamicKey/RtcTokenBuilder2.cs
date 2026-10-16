using NovaLive.Application.Abstractions.ThirdParty;

namespace NovaLive.Infrastructure.Livestreams.Agora.DynamicKey;

public static class RtcTokenBuilder2
{
    public static string BuildTokenWithUserAccount(
        string appId,
        string appCertificate,
        string channelName,
        string account,
        AgoraRtcRole role,
        uint tokenExpire,
        uint privilegeExpire,
        uint? issueTs = null,
        uint? salt = null)
    {
        uint currentTs = issueTs ?? (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        uint randomSalt = salt ?? (uint)Random.Shared.Next();

        var accessToken = new AccessToken2(appId, appCertificate, tokenExpire, currentTs, randomSalt);
        var serviceRtc = new AccessToken2.ServiceRtc(channelName, account);

        serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegeJoinChannel, privilegeExpire);

        if (role == AgoraRtcRole.Publisher)
        {
            serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegePublishAudioStream, privilegeExpire);
            serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegePublishVideoStream, privilegeExpire);
            serviceRtc.AddPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PrivilegePublishDataStream, privilegeExpire);
        }

        accessToken.AddService(serviceRtc);
        return accessToken.Build();
    }
}
