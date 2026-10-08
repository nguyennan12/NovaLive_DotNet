namespace NovaLive.Contracts.V1.Users;

public record UserProfileResponse(
    Guid Id,
    string Email,
    string Phone,
    string FullName,
    DateTime? Birthday,
    string? Gender,
    string? AvatarUrl,
    string AccountStatus,
    DateTime CreatedAt,
    List<UserAddressResponse> Addresses);

public record UserAddressResponse(
    Guid Id,
    string RecipientName,
    string Phone,
    string ProvinceName,
    string DistrictName,
    string WardName,
    string DetailAddress,
    bool IsDefault);
