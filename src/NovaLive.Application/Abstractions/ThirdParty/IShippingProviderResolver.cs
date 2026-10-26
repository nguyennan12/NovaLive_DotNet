using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IShippingProviderResolver
{
    Result<IShippingProvider> Resolve(string provider);
}
