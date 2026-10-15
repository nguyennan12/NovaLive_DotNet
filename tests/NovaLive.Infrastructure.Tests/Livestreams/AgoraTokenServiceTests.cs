using FluentAssertions;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Infrastructure.Livestreams.Agora;
using NovaLive.Infrastructure.Livestreams.Agora.DynamicKey;

namespace NovaLive.Infrastructure.Tests.Livestreams;

public class AgoraTokenServiceTests
{
    private const string ValidAppId = "970ca35de60c44645bbae8a215061b33";
    private const string ValidAppCertificate = "5cfd2fd1755d40ecb72977518be15d3b";
    private const string ValidChannelName = "live_shop123_session456";

    private readonly AgoraOptions _defaultOptions = new()
    {
        AppId = ValidAppId,
        AppCertificate = ValidAppCertificate,
        TokenTtlSeconds = 7200
    };

    [Fact]
    public void GeneratePublisherToken_WithValidUserId_ReturnsValid007TokenWithAllPrivileges()
    {
        var sut = CreateSut(_defaultOptions);
        var userId = Guid.NewGuid().ToString();

        var token = sut.GeneratePublisherToken(ValidChannelName, userId);

        token.Should().NotBeNullOrWhiteSpace();
        token.Should().StartWith("007");

        var parsed = new AccessToken2();
        parsed.Parse(token).Should().BeTrue();
        parsed.VerifySignature(ValidAppCertificate).Should().BeTrue();
        parsed.AppId.Should().Be(ValidAppId);

        var rtcServices = parsed.GetServices(AccessToken2.ServiceTypeRtc);
        rtcServices.Should().HaveCount(1);

        var rtc = (AccessToken2.ServiceRtc)rtcServices[0];
        rtc.ChannelName.Should().Be(ValidChannelName);
        rtc.Uid.Should().Be(userId);

        var privileges = rtc.GetPrivileges();
        privileges.Should().ContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegeJoinChannel);
        privileges.Should().ContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegePublishAudioStream);
        privileges.Should().ContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegePublishVideoStream);
        privileges.Should().ContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegePublishDataStream);
        privileges[(ushort)AccessToken2.PrivilegeRtcEnum.PrivilegeJoinChannel].Should().Be(7200);
    }

    [Fact]
    public void GenerateSubscriberToken_WithGuestUserString_ReturnsValid007TokenWithJoinOnly()
    {
        var sut = CreateSut(_defaultOptions);
        const string guestUserId = "guest_session_987654";

        var token = sut.GenerateSubscriberToken(ValidChannelName, guestUserId);

        token.Should().NotBeNullOrWhiteSpace();
        token.Should().StartWith("007");

        var parsed = new AccessToken2();
        parsed.Parse(token).Should().BeTrue();
        parsed.VerifySignature(ValidAppCertificate).Should().BeTrue();

        var rtc = (AccessToken2.ServiceRtc)parsed.GetServices(AccessToken2.ServiceTypeRtc).Single();
        rtc.ChannelName.Should().Be(ValidChannelName);
        rtc.Uid.Should().Be(guestUserId);

        var privileges = rtc.GetPrivileges();
        privileges.Should().ContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegeJoinChannel);
        privileges.Should().NotContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegePublishAudioStream);
        privileges.Should().NotContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegePublishVideoStream);
        privileges.Should().NotContainKey((ushort)AccessToken2.PrivilegeRtcEnum.PrivilegePublishDataStream);
        privileges.Should().HaveCount(1);
    }

    [Fact]
    public void OfficialAgoraAccessToken2_OldToken_ParsesAndVerifiesSuccessfully()
    {
        const string oldToken = "007eJxTYBBbsMMnKq7p9Hf/HcIX5kce9b518kCiQgSr5Zrp4X1Tu6UUGCzNDZwdjU1TUs0Mkk1MzExMk5ISUy0SjQxNDcwMk4yN3b8IMEQwMTAwMoAwBIL4CgzmKeZGxmamqUmWFsYmFqbGluapxqnGaZYpJmYGSSkpiVwMRhYWRsYmhkbmxgDCaiTj";
        const string appCertificate = "5CFd2fd1755d40ecb72977518be15d3b";

        var parsed = new AccessToken2();
        parsed.Parse(oldToken).Should().BeTrue();
        parsed.VerifySignature(appCertificate).Should().BeTrue();
        parsed.GetServices(AccessToken2.ServiceTypeRtc).Should().HaveCount(1);
    }

    [Fact]
    public void DeterministicTokenGeneration_MatchesExactParameters()
    {
        const uint fixedTs = 1600000000;
        const uint fixedSalt = 12345678;
        const uint expire = 3600;

        var token = RtcTokenBuilder2.BuildTokenWithUserAccount(
            ValidAppId,
            ValidAppCertificate,
            ValidChannelName,
            "deterministic_user",
            AgoraRtcRole.Publisher,
            expire,
            expire,
            fixedTs,
            fixedSalt);

        token.Should().StartWith("007");

        var parsed = new AccessToken2();
        parsed.Parse(token).Should().BeTrue();
        parsed.VerifySignature(ValidAppCertificate).Should().BeTrue();
        parsed.IssueTs.Should().Be(fixedTs);
        parsed.Salt.Should().Be(fixedSalt);
        parsed.Expire.Should().Be(expire);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateToken_WithEmptyChannelName_ThrowsArgumentException(string? invalidChannel)
    {
        var sut = CreateSut(_defaultOptions);
        var act = () => sut.GeneratePublisherToken(invalidChannel!, "user_123");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*channelName*");
    }

    [Fact]
    public void GenerateToken_WithChannelNameExceeding64Bytes_ThrowsArgumentException()
    {
        var sut = CreateSut(_defaultOptions);
        var tooLongChannel = new string('a', 65);

        var act = () => sut.GeneratePublisherToken(tooLongChannel, "user_123");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*channelName*");
    }

    [Fact]
    public void GenerateToken_WithInvalidChannelNameCharacters_ThrowsArgumentException()
    {
        var sut = CreateSut(_defaultOptions);
        const string invalidChannel = "channel/with/slashes";

        var act = () => sut.GeneratePublisherToken(invalidChannel, "user_123");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*channelName*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GenerateToken_WithEmptyUserId_ThrowsArgumentException(string? invalidUser)
    {
        var sut = CreateSut(_defaultOptions);
        var act = () => sut.GeneratePublisherToken(ValidChannelName, invalidUser!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*userId*");
    }

    [Fact]
    public void GenerateToken_WithInvalidAppId_ThrowsInvalidOperationException()
    {
        var invalidOptions = new AgoraOptions
        {
            AppId = "invalid_short_id",
            AppCertificate = ValidAppCertificate
        };
        var sut = CreateSut(invalidOptions);

        var act = () => sut.GeneratePublisherToken(ValidChannelName, "user_123");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AppId*");
    }

    [Fact]
    public void GenerateToken_WithInvalidAppCertificate_ThrowsInvalidOperationException()
    {
        var invalidOptions = new AgoraOptions
        {
            AppId = ValidAppId,
            AppCertificate = "not_hex_cert"
        };
        var sut = CreateSut(invalidOptions);

        var act = () => sut.GeneratePublisherToken(ValidChannelName, "user_123");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AppCertificate*");
    }

    [Fact]
    public void GenerateToken_WithCustomTtl_AppliesCustomTtl()
    {
        var sut = CreateSut(_defaultOptions);
        var customTtl = TimeSpan.FromMinutes(30);

        var token = sut.GeneratePublisherToken(ValidChannelName, "user_123", customTtl);

        var parsed = new AccessToken2();
        parsed.Parse(token).Should().BeTrue();
        parsed.Expire.Should().Be(1800);
    }

    private static AgoraTokenService CreateSut(AgoraOptions options)
    {
        return new AgoraTokenService(Options.Create(options));
    }
}
