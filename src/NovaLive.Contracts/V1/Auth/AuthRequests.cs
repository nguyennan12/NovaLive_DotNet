namespace NovaLive.Contracts.V1.Auth;

public record RegisterUserRequest(
    string Email,
    string Phone,
    string Password,
    string FullName);

public record VerifyOtpRequest(
    Guid UserId,
    string OtpCode);

public record ResendOtpRequest(
    string EmailOrPhone);

public record LoginRequest(
    string EmailOrPhone,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword);

public record ForgotPasswordRequest(
    string Email);

public record ResetPasswordRequest(
    string Email,
    string OtpCode,
    string NewPassword);
