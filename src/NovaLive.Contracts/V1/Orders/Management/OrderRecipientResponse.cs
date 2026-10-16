namespace NovaLive.Contracts.V1.Orders;
public sealed record OrderRecipientResponse(string RecipientName, string Phone, int? ProvinceId, string ProvinceName, int? DistrictId, string DistrictName, string? WardCode, string WardName, string DetailAddress);
