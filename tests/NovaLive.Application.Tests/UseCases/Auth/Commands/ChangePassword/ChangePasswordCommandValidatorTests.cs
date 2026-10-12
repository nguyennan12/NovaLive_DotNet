using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.ChangePassword;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new ChangePasswordCommandValidator()
        .Validate(new ChangePasswordCommand(new("Password123", "NewPassword123"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new ChangePasswordCommandValidator()
        .Validate(new ChangePasswordCommand(new("", "weak"))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new ChangePasswordCommandValidator()
        .Validate(new ChangePasswordCommand(null!)).IsValid.Should().BeFalse();
}
