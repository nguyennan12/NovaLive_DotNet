using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.ThirdParty;

public interface IShippingProvider
{
    string ProviderCode { get; }

    Task<Result<ShippingFeeQuote>> CalculateFeeAsync(ShippingFeeCalculationModel request, CancellationToken cancellationToken = default);

    Task<Result<ShippingOrderCreateResult>> CreateShipmentAsync(ShippingShipmentCreateModel request, CancellationToken cancellationToken = default);

    Task<Result<byte[]>> GetShippingLabelAsync(string trackingCode, CancellationToken cancellationToken = default);
}

public sealed record ShippingFeeRequest(Guid ShopId, Guid WarehouseAddressId, Guid ShippingAddressId, int WeightGram);

public sealed record ShippingFeeCalculationModel(
    int FromDistrictId,
    string FromWardCode,
    int ToDistrictId,
    string ToWardCode,
    int WeightGram,
    int LengthCm = 10,
    int WidthCm = 10,
    int HeightCm = 10,
    decimal InsuranceValue = 0,
    int? ServiceTypeId = null);

public sealed record ShippingFeeQuote(
    string ProviderCode,
    string ServiceCode,
    decimal TotalFee,
    DateTime? ExpectedDeliveryDate = null);

public sealed record ShippingShipmentCreateModel(
    string OrderCode,
    string FromName,
    string FromPhone,
    string FromAddress,
    string FromWardName,
    string FromDistrictName,
    int FromDistrictId,
    string FromWardCode,
    string ToName,
    string ToPhone,
    string ToAddress,
    string ToWardCode,
    int ToDistrictId,
    decimal CodAmount,
    int WeightGram,
    string RequiredNote,
    List<ShippingItemModel> Items,
    int LengthCm = 10,
    int WidthCm = 10,
    int HeightCm = 10,
    int? ServiceTypeId = null);

public sealed record ShippingItemModel(
    string Name,
    string Code,
    int Quantity,
    int WeightGram);

public sealed record ShippingOrderCreateResult(
    string ProviderOrderCode,
    string TrackingCode,
    decimal TotalFee,
    DateTime? ExpectedDeliveryDate = null);
