using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Auth.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized("Auth.InvalidCredentials", "Invalid credentials.");
    public static readonly Error InvalidRefresh = Error.Unauthorized("Auth.InvalidRefreshToken", "Invalid refresh token.");
    public static readonly Error InvalidOtp = new(ErrorType.Invalid, "Auth.InvalidOtp", "Invalid or expired verification code.");
    public static readonly Error NotVerified = Error.Forbidden("Auth.AccountNotVerified", "Verify your account before signing in.");
    public static readonly Error Inactive = Error.Forbidden("Auth.AccountInactive", "This account cannot sign in.");
    public static readonly Error Duplicate = Error.Conflict("Auth.DuplicateAccount", "Email or phone is already registered.");
    public static Error Limited(int seconds) => new(ErrorType.TooManyRequests, "Auth.RateLimited", "Please try again later.")
        { RetryAfterSeconds = Math.Max(1, seconds) };
    public static Error Locked(int seconds) => new(ErrorType.Locked, "Auth.AccountLocked", "Sign in is temporarily locked.")
        { RetryAfterSeconds = Math.Max(1, seconds) };
}
