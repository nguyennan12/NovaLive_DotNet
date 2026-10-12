namespace NovaLive.Contracts.V1.Users;

public record UpdateProfileRequest(
    string FullName,
    string? Phone,
    DateOnly? Birthday,
    string? Gender,
    string? AvatarUrl);

public record CreateAddressRequest(
    string RecipientName,
    string Phone,
    int? ProvinceId,
    string ProvinceName,
    int? DistrictId,
    string DistrictName,
    string? WardCode,
    string WardName,
    string DetailAddress,
    bool IsDefault = false);

public record UpdateAddressRequest(
    string RecipientName,
    string Phone,
    int? ProvinceId,
    string ProvinceName,
    int? DistrictId,
    string DistrictName,
    string? WardCode,
    string WardName,
    string DetailAddress,
    bool IsDefault = false);

public record UpdateUserProfileRequest(
    string FullName,
    string? Phone,
    DateOnly? Birthday,
    string? Gender,
    string? AvatarUrl) : UpdateProfileRequest(FullName, Phone, Birthday, Gender, AvatarUrl);

public record CreateUserAddressRequest(
    string RecipientName,
    string Phone,
    int? ProvinceId,
    string ProvinceName,
    int? DistrictId,
    string DistrictName,
    string? WardCode,
    string WardName,
    string DetailAddress,
    bool IsDefault = false) : CreateAddressRequest(RecipientName, Phone, ProvinceId, ProvinceName, DistrictId, DistrictName, WardCode, WardName, DetailAddress, IsDefault);

public record UpdateUserAddressRequest(
    string RecipientName,
    string Phone,
    int? ProvinceId,
    string ProvinceName,
    int? DistrictId,
    string DistrictName,
    string? WardCode,
    string WardName,
    string DetailAddress,
    bool IsDefault = false) : UpdateAddressRequest(RecipientName, Phone, ProvinceId, ProvinceName, DistrictId, DistrictName, WardCode, WardName, DetailAddress, IsDefault);
