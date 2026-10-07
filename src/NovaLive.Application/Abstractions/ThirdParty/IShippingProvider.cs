namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IShippingProvider
{
    string ProviderCode { get; }

    Task<ShippingFeeQuote> CalculateFeeAsync(ShippingFeeRequest request, CancellationToken cancellationToken = default);
}

public sealed record ShippingFeeRequest(Guid ShopId, Guid WarehouseAddressId, Guid ShippingAddressId, int WeightGram);

public sealed record ShippingFeeQuote(string ProviderCode, string ServiceCode, decimal Fee);
