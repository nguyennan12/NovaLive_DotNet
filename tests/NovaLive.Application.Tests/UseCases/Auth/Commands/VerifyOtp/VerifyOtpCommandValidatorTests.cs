using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.VerifyOtp;

public sealed class VerifyOtpCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new VerifyOtpCommandValidator()
        .Validate(new VerifyOtpCommand(new(Guid.NewGuid(), "123456"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new VerifyOtpCommandValidator()
        .Validate(new VerifyOtpCommand(new(Guid.Empty, "abcd"))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new VerifyOtpCommandValidator()
        .Validate(new VerifyOtpCommand(null!)).IsValid.Should().BeFalse();
}
