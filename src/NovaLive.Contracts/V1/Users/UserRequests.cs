namespace NovaLive.Contracts.V1.Users;

public record UpdateUserProfileRequest(
    string FullName,
    string Phone,
    DateTime? Birthday,
    string? Gender,
    string? AvatarUrl);

public record CreateUserAddressRequest(
    string RecipientName,
    string Phone,
    string ProvinceName,
    string DistrictName,
    string WardName,
    string DetailAddress,
    bool IsDefault);

public record UpdateUserAddressRequest(
    string RecipientName,
    string Phone,
    string ProvinceName,
    string DistrictName,
    string WardName,
    string DetailAddress,
    bool IsDefault);
