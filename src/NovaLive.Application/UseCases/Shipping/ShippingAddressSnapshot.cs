namespace NovaLive.Application.UseCases.Shipping;

public sealed record ShippingAddressSnapshot(
    string RecipientName, string Phone, int? ProvinceId, string ProvinceName,
    int? DistrictId, string DistrictName, string? WardCode, string WardName, string DetailAddress);
