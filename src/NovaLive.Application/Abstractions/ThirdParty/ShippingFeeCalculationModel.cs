namespace NovaLive.Application.Abstractions.ThirdParty;

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
