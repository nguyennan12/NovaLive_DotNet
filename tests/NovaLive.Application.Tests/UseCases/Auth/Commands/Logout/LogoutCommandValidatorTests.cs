using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.Logout;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.Logout;

public sealed class LogoutCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new LogoutCommandValidator()
        .Validate(new LogoutCommand(new("refresh-token"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new LogoutCommandValidator()
        .Validate(new LogoutCommand(new(""))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new LogoutCommandValidator()
        .Validate(new LogoutCommand(null!)).IsValid.Should().BeFalse();
}
