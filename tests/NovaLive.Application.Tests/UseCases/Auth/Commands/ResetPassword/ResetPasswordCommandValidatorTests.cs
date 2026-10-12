using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.ResetPassword;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new ResetPasswordCommandValidator()
        .Validate(new ResetPasswordCommand(new("buyer@example.com", "123456", "NewPassword123"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new ResetPasswordCommandValidator()
        .Validate(new ResetPasswordCommand(new("bad", "wrong", "weak"))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new ResetPasswordCommandValidator()
        .Validate(new ResetPasswordCommand(null!)).IsValid.Should().BeFalse();
}
