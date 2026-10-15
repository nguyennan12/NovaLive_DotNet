using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Infrastructure.Livestreams.Agora.DynamicKey;

namespace NovaLive.Infrastructure.Livestreams.Agora;

public sealed partial class AgoraTokenService : IAgoraTokenService
{
    private readonly AgoraOptions _options;
    private readonly TimeProvider _timeProvider;

    [GeneratedRegex(@"^[a-zA-Z0-9!#$%&()+\-:;<=.>?@\[\]^_{}|~, ]{1,64}$")]
    private static partial Regex ChannelNameRegex();

    public AgoraTokenService(IOptions<AgoraOptions> options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public string GeneratePublisherToken(string channelName, string userId, TimeSpan? ttl = null)
    {
        return GenerateToken(channelName, userId, AgoraRtcRole.Publisher, ttl);
    }

    public string GenerateSubscriberToken(string channelName, string userId, TimeSpan? ttl = null)
    {
        return GenerateToken(channelName, userId, AgoraRtcRole.Subscriber, ttl);
    }

    public string GenerateToken(string channelName, string userId, AgoraRtcRole role, TimeSpan? ttl = null)
    {
        ValidateChannelName(channelName);
        ValidateUserId(userId);
        ValidateCredentials();

        uint expireSeconds = ttl.HasValue && ttl.Value > TimeSpan.Zero
            ? (uint)Math.Min(ttl.Value.TotalSeconds, 86400)
            : (uint)Math.Clamp(_options.TokenTtlSeconds, 60, 86400);

        uint currentTs = (uint)_timeProvider.GetUtcNow().ToUnixTimeSeconds();
        uint salt = (uint)Random.Shared.Next();

        return RtcTokenBuilder2.BuildTokenWithUserAccount(
            _options.AppId,
            _options.AppCertificate,
            channelName,
            userId,
            role,
            tokenExpire: expireSeconds,
            privilegeExpire: expireSeconds,
            issueTs: currentTs,
            salt: salt);
    }

    private static void ValidateChannelName(string? channelName)
    {
        if (string.IsNullOrWhiteSpace(channelName))
        {
            throw new ArgumentException("Mã kênh (channelName) không được để trống.", nameof(channelName));
        }

        int byteCount = Encoding.UTF8.GetByteCount(channelName);
        if (byteCount is < 1 or > 64 || !ChannelNameRegex().IsMatch(channelName))
        {
            throw new ArgumentException("Mã kênh (channelName) không hợp lệ theo quy định của Agora (tối đa 64 byte và chỉ chứa các ký tự hợp lệ).", nameof(channelName));
        }
    }

    private static void ValidateUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Mã người dùng (userId) không được để trống.", nameof(userId));
        }

        if (Encoding.UTF8.GetByteCount(userId) > 255)
        {
            throw new ArgumentException("Mã người dùng (userId) không được vượt quá 255 byte.", nameof(userId));
        }
    }

    private void ValidateCredentials()
    {
        if (!DynamicKeyUtil.IsHex32(_options.AppId))
        {
            throw new InvalidOperationException("Cấu hình Agora AppId không hợp lệ (phải gồm đúng 32 ký tự hex).");
        }

        if (!DynamicKeyUtil.IsHex32(_options.AppCertificate))
        {
            throw new InvalidOperationException("Cấu hình Agora AppCertificate không hợp lệ (phải gồm đúng 32 ký tự hex).");
        }
    }
}
