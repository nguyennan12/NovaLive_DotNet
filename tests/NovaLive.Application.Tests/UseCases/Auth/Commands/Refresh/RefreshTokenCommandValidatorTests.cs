using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Commands.Refresh;

namespace NovaLive.Application.Tests.UseCases.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandValidatorTests
{
    [Fact]
    public void ValidRequest_IsAccepted() => new RefreshTokenCommandValidator()
        .Validate(new RefreshTokenCommand(new("refresh-token"))).IsValid.Should().BeTrue();

    [Fact]
    public void InvalidRequest_IsRejected() => new RefreshTokenCommandValidator()
        .Validate(new RefreshTokenCommand(new(""))).IsValid.Should().BeFalse();

    [Fact]
    public void NullData_IsRejectedWithoutThrowing() => new RefreshTokenCommandValidator()
        .Validate(new RefreshTokenCommand(null!)).IsValid.Should().BeFalse();
}
