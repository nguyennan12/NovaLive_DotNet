namespace NovaLive.Contracts.V1.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserSummaryDto User);

public record RegisterUserResponse(
    Guid UserId,
    string Email,
    string Status,
    DateTime OtpExpiresAt);

public record UserSummaryDto(
    Guid Id,
    string Email,
    string Phone,
    string FullName,
    string? AvatarUrl,
    string AccountStatus,
    Guid? ShopId,
    List<string> Roles);
