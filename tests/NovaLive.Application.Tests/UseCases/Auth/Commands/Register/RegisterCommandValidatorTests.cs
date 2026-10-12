using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.Register;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.Register;

public sealed class RegisterCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new RegisterCommandValidator()
        .Validate(new RegisterCommand(new("buyer@example.com", "0912345678", "Password123", "Buyer"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new RegisterCommandValidator()
        .Validate(new RegisterCommand(new("bad", "111", "weak", ""))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new RegisterCommandValidator()
        .Validate(new RegisterCommand(null!)).IsValid.Should().BeFalse();
}
