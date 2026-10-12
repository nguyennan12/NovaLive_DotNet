namespace NovaLive.Contracts.V1.Users;

public record UserProfileResponse(
    Guid Id,
    string Email,
    string? Phone,
    string FullName,
    DateOnly? Birthday,
    string? Gender,
    string? AvatarUrl,
    string AccountStatus,
    DateTimeOffset CreatedAt,
    IReadOnlyList<AddressResponse>? Addresses = null);

public record AddressResponse(
    Guid Id,
    Guid UserId,
    string RecipientName,
    string Phone,
    int? ProvinceId,
    string ProvinceName,
    int? DistrictId,
    string DistrictName,
    string? WardCode,
    string WardName,
    string DetailAddress,
    bool IsDefault,
    DateTimeOffset CreatedAt);

public record UserAddressResponse(
    Guid Id,
    Guid UserId,
    string RecipientName,
    string Phone,
    int? ProvinceId,
    string ProvinceName,
    int? DistrictId,
    string DistrictName,
    string? WardCode,
    string WardName,
    string DetailAddress,
    bool IsDefault,
    DateTimeOffset CreatedAt) : AddressResponse(Id, UserId, RecipientName, Phone, ProvinceId, ProvinceName, DistrictId, DistrictName, WardCode, WardName, DetailAddress, IsDefault, CreatedAt);
