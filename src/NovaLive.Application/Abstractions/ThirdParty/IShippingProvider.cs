using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IShippingProvider
{
    string ProviderCode { get; }

    Task<Result<ShippingFeeQuote>> CalculateFeeAsync(ShippingFeeCalculationModel request, CancellationToken cancellationToken = default);

    Task<Result<ShippingOrderCreateResult>> CreateShipmentAsync(ShippingShipmentCreateModel request, CancellationToken cancellationToken = default);

    Task<Result<byte[]>> GetShippingLabelAsync(string trackingCode, CancellationToken cancellationToken = default);

    Task<Result<ShippingTrackingResult>> GetTrackingAsync(string trackingCode, CancellationToken cancellationToken = default);
}
