using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.UseCases.Auth.Commands.Register;
using NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpWorkflowTests
{
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
        var registration = await host.Send(new RegisterCommand(new("new@example.com", "0912345678", "Password123", "New")));
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
}
