using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.ResendOtp;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.ResendOtp;

public sealed class ResendOtpCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new ResendOtpCommandValidator()
        .Validate(new ResendOtpCommand(new("buyer@example.com"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new ResendOtpCommandValidator()
        .Validate(new ResendOtpCommand(new(""))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new ResendOtpCommandValidator()
        .Validate(new ResendOtpCommand(null!)).IsValid.Should().BeFalse();
}
