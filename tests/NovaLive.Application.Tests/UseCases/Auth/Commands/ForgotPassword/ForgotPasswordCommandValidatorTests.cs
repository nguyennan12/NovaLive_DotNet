using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.ForgotPassword;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new ForgotPasswordCommandValidator()
        .Validate(new ForgotPasswordCommand(new("buyer@example.com"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new ForgotPasswordCommandValidator()
        .Validate(new ForgotPasswordCommand(new("bad"))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new ForgotPasswordCommandValidator()
        .Validate(new ForgotPasswordCommand(null!)).IsValid.Should().BeFalse();
}
