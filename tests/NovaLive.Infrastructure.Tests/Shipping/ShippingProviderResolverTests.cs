using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Infrastructure.Shipping;

namespace NovaLive.Infrastructure.Tests.Shipping;

public sealed class ShippingProviderResolverTests
{
    [Theory]
    [InlineData("GHN")]
    [InlineData("GHTK")]
    [InlineData("ViettelPost")]
    public void RegisteredProvider_IsResolvedByCode(string code)
    {
        var provider = new Mock<IShippingProvider>(); provider.SetupGet(p => p.ProviderCode).Returns(code);
        new ShippingProviderResolver([provider.Object]).Resolve(code).Value.Should().BeSameAs(provider.Object);
    }

    [Theory]
    [InlineData("GHTK")]
    [InlineData("ViettelPost")]
    public void KnownUnregisteredProvider_IsUnavailable(string code) =>
        new ShippingProviderResolver([]).Resolve(code).Error.Should().Be(ShippingErrors.ProviderNotAvailable);

    [Theory]
    [InlineData("invalid")]
    [InlineData("0")]
    [InlineData("")]
    public void UnknownProvider_IsUnsupported(string code) =>
        new ShippingProviderResolver([]).Resolve(code).Error.Should().Be(ShippingErrors.UnsupportedProvider);
}
