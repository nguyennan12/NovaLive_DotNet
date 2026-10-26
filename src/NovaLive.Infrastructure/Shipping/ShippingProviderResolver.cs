using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Shipping;

public sealed class ShippingProviderResolver(IEnumerable<IShippingProvider> providers) : IShippingProviderResolver
{
    public Result<IShippingProvider> Resolve(string provider)
    {
        if (provider is not ("GHN" or "GHTK" or "ViettelPost")) return ShippingErrors.UnsupportedProvider;
        var adapter = providers.SingleOrDefault(p => p.ProviderCode == provider);
        if (adapter is null) return ShippingErrors.ProviderNotAvailable;
        return Result<IShippingProvider>.Success(adapter);
    }
}
