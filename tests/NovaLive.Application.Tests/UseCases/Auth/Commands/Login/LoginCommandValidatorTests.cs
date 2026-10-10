using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.Login;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.Login;

public sealed class LoginCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new LoginCommandValidator()
        .Validate(new LoginCommand(new("buyer@example.com", "Password123"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new LoginCommandValidator()
        .Validate(new LoginCommand(new("", ""))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new LoginCommandValidator()
        .Validate(new LoginCommand(null!)).IsValid.Should().BeFalse();
}
