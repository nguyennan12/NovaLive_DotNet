using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Auth;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class AuthWorkflowTests
{
    [Fact]
    public async Task Registration_NormalizesEmail_HashesOtp_SendsMail_AndRejectsDuplicateEmailOrPhone()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var result = await host.Send(new RegisterUserCommand(new("  New@Example.com  ", "0912345678", "Password123", "New User")));
        result.IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            var user = await db.Users.SingleAsync();
            user.Email.Should().Be("new@example.com");
            user.AccountStatus.Should().Be(AccountStatus.Unverified);
            user.PasswordHash.Should().NotBe("Password123");
            var otp = await db.UserOtps.SingleAsync();
            otp.OtpHash.Should().HaveLength(64).And.NotBe(host.Codes[user.Email]);
            otp.ExpiresAt.Should().Be(host.Clock.UtcNow.AddMinutes(5));
        });
        host.Mail.Verify(m => m.SendOtpAsync("new@example.com", It.IsAny<string>(), OtpType.EmailVerify, It.IsAny<CancellationToken>()), Times.Once);
        (await host.Send(new RegisterUserCommand(new("NEW@example.com", "0987654321", "Password123", "Duplicate")))).Error.Type.Should().Be(ErrorType.AlreadyExists);
        (await host.Send(new RegisterUserCommand(new("other@example.com", "+84912345678", "Password123", "Duplicate")))).Error.Type.Should().Be(ErrorType.AlreadyExists);
    }
    [Fact]
    public async Task EmailFailure_DoesNotRollBackRegistration()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        host.Mail.Setup(m => m.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OtpType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email provider unavailable"));
        (await host.Send(new RegisterUserCommand(new("new@example.com", "0912345678", "Password123", "New")))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db => (await db.Users.CountAsync()).Should().Be(1));
    }
    [Theory]
    [InlineData("valid")]
    [InlineData("wrong")]
    [InlineData("expired")]
    [InlineData("used")]
    [InlineData("attempts")]
    public async Task VerifyOtp_EnforcesCodeStateAndAttemptBudget(string scenario)
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var registration = await host.Send(new RegisterUserCommand(new("new@example.com", "0912345678", "Password123", "New")));
        var id = registration.Value!.UserId;
        var code = host.Codes["new@example.com"];
        if (scenario == "expired") host.Clock.UtcNow += TimeSpan.FromMinutes(6);
        if (scenario == "used") await host.WithDb(async db => { (await db.UserOtps.SingleAsync()).MarkUsed(host.Clock.UtcNow); await db.SaveChangesAsync(); });
        if (scenario == "attempts")
            for (var i = 0; i < 5; i++) await host.Send(new VerifyOtpCommand(new(id, code == "000000" ? "111111" : "000000")));
        var result = await host.Send(new VerifyOtpCommand(new(id, scenario == "wrong" ? (code == "000000" ? "111111" : "000000") : code)));
        if (scenario == "valid")
        {
            result.IsSuccess.Should().BeTrue();
            await host.WithDb(async db =>
            {
                (await db.Users.SingleAsync()).AccountStatus.Should().Be(AccountStatus.Active);
                (await db.UserOtps.SingleAsync()).UsedAt.Should().NotBeNull();
                (await db.UserRoles.CountAsync(role => role.UserId == id && role.RoleId == SystemRoleIds.Buyer)).Should().Be(1);
            });
        }
        else result.Error.Type.Should().Be(scenario == "attempts" ? ErrorType.TooManyRequests : ErrorType.Invalid);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Resend_FourthRequestIsLimited_RegardlessOfAccountExistence(bool exists)
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        if (exists) await host.AddUser(active: false);
        for (var i = 0; i < 3; i++)
            (await host.Send(new ResendOtpCommand(new("buyer@example.com")))).IsSuccess.Should().BeTrue();
        var result = await host.Send(new ResendOtpCommand(new("buyer@example.com")));
        result.Error.Type.Should().Be(ErrorType.TooManyRequests);
        result.Error.RetryAfterSeconds.Should().Be(900);
    }
    [Fact]
    public async Task Login_UsesAllBackoffSteps_AndDoesNotCheckPasswordWhileLocked()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        foreach (var minutes in new[] { 5, 15, 60, 1440 })
        {
            for (var i = 1; i < 5; i++)
                (await host.Send(new LoginCommand(new("buyer@example.com", "wrong")))).Error.Type.Should().Be(ErrorType.Unauthorized);
            var locked = await host.Send(new LoginCommand(new("buyer@example.com", "wrong")));
            locked.Error.Type.Should().Be(ErrorType.Locked);
            locked.Error.RetryAfterSeconds.Should().Be(minutes * 60);
            host.Passwords.Invocations.Clear();
            (await host.Send(new LoginCommand(new("buyer@example.com", "Password123")))).Error.Type.Should().Be(ErrorType.Locked);
            host.Passwords.Verify(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            host.Clock.UtcNow += TimeSpan.FromMinutes(minutes);
        }
    }
    [Fact]
    public async Task Login_SuccessClearsCounters_AndStoresOnlyRefreshHash()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        await host.Send(new LoginCommand(new("buyer@example.com", "wrong")));
        await host.Cache.SetAsync("login:lockcount:buyer@example.com", 2L, TimeSpan.FromDays(1));
        var result = await host.Send(new LoginCommand(new("buyer@example.com", "Password123"), "127.0.0.1", new string('x', 700)));
        result.IsSuccess.Should().BeTrue();
        (await host.Cache.ExistsAsync("login:failed:buyer@example.com")).Should().BeFalse();
        (await host.Cache.ExistsAsync("login:lockcount:buyer@example.com")).Should().BeFalse();
        await host.WithDb(async db =>
        {
            var token = await db.RefreshTokens.SingleAsync();
            token.TokenHash.Should().NotBe(result.Value!.RefreshToken).And.HaveLength(64);
            token.DeviceInfo.Should().HaveLength(500);
            token.ExpiresAt.Should().Be(host.Clock.UtcNow.AddDays(30));
        });
    }
    [Fact]
    public async Task Login_UnverifiedIsForbidden_UnknownAndWrongUseSameError()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser(active: false);
        (await host.Send(new LoginCommand(new("buyer@example.com", "Password123")))).Error.Should().Be(AuthErrors.NotVerified);
        var unknown = await host.Send(new LoginCommand(new("unknown@example.com", "wrong")));
        var wrong = await host.Send(new LoginCommand(new("buyer@example.com", "wrong")));
        unknown.Error.Should().Be(wrong.Error);
        host.Passwords.Verify(p => p.VerifyDummy("wrong"), Times.Once);
    }
    [Fact]
    public async Task Refresh_Rotates_AndReuseCommitsRevocationIncludingNewToken()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        var login = await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        var rotation = await host.Send(new RefreshTokenCommand(new(login.Value!.RefreshToken)));
        rotation.IsSuccess.Should().BeTrue();
        rotation.Value!.RefreshToken.Should().NotBe(login.Value.RefreshToken);
        var reuse = await host.Send(new RefreshTokenCommand(new(login.Value.RefreshToken)));
        reuse.Error.Type.Should().Be(ErrorType.Unauthorized);
        await host.WithDb(async db => (await db.RefreshTokens.ToListAsync()).Should().HaveCount(2).And.OnlyContain(t => t.RevokedAt != null));
        (await host.Send(new RefreshTokenCommand(new(rotation.Value.RefreshToken)))).Error.Type.Should().Be(ErrorType.Unauthorized);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ForgotPassword_ReturnsSameSuccessEvenWhenThrottled(bool exists)
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        if (exists) await host.AddUser();
        for (var i = 0; i < 4; i++)
            (await host.Send(new ForgotPasswordCommand(new("buyer@example.com")))).IsSuccess.Should().BeTrue();
        host.Mail.Verify(m => m.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), OtpType.ForgotPassword, It.IsAny<CancellationToken>()),
            Times.Exactly(exists ? 3 : 0));
    }
    [Fact]
    public async Task ChangePassword_RevokesAllRefreshTokensAndBlacklistsCurrentJti()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        host.Authenticate(await host.AddUser());
        await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        (await host.Send(new ChangePasswordCommand(new("wrong", "NewPassword123")))).Error.Type.Should().Be(ErrorType.Invalid);
        (await host.Send(new ChangePasswordCommand(new("Password123", "Password123")))).Error.Type.Should().Be(ErrorType.Invalid);
        (await host.Send(new ChangePasswordCommand(new("Password123", "NewPassword123")))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db => (await db.RefreshTokens.ToListAsync()).Should().OnlyContain(t => t.RevokedAt != null));
        (await host.Cache.GetTimeToLiveAsync("Blacklist:" + host.CurrentUser.Object.Jti)).Should().Be(TimeSpan.FromMinutes(12));
    }
    [Fact]
    public async Task ResetPassword_ConsumesOtp_RevokesTokens_AndClearsLoginLock()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        await host.AddUser();
        await host.Send(new LoginCommand(new("buyer@example.com", "Password123")));
        await host.Send(new ForgotPasswordCommand(new("buyer@example.com")));
        await host.Cache.SetAsync("login:lock:buyer@example.com", true, TimeSpan.FromMinutes(5));
        (await host.Send(new ResetPasswordCommand(new("buyer@example.com", host.Codes["buyer@example.com"], "NewPassword123")))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.RefreshTokens.ToListAsync()).Should().OnlyContain(t => t.RevokedAt != null);
            (await db.Users.SingleAsync()).PasswordHash.Should().Be("hash:NewPassword123");
        });
        (await host.Cache.ExistsAsync("login:lock:buyer@example.com")).Should().BeFalse();
    }
    [Fact]
    public async Task Logout_BlacklistsForRemainingTtl_AndDoesNotRevokeAnotherUsersToken()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        host.Authenticate(await host.AddUser());
        await host.AddUser("other@example.com", phone: "0987654321");
        var otherLogin = await host.Send(new LoginCommand(new("other@example.com", "Password123")));
        (await host.Send(new LogoutCommand(new(otherLogin.Value!.RefreshToken)))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db => (await db.RefreshTokens.SingleAsync()).RevokedAt.Should().BeNull());
        (await host.Cache.GetTimeToLiveAsync("Blacklist:" + host.CurrentUser.Object.Jti)).Should().Be(TimeSpan.FromMinutes(12));
    }
}
