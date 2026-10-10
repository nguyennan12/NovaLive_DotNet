using NovaLive.Domain.Common;
namespace NovaLive.Application.Abstractions.Auth;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
    void VerifyDummy(string password);
}
public sealed record AccessToken(string Value, string Jti, DateTimeOffset ExpiresAt);
public interface IJwtTokenService
{
    AccessToken Create(Guid userId, string email, IReadOnlyCollection<string> roles, Guid? shopId);
}
public sealed record RefreshTokenValue(string Value, string Hash);
public interface IRefreshTokenService
{
    RefreshTokenValue Create();
    string Hash(string token);
}
public interface IOtpService
{
    string Generate();
    string Hash(Guid userId, OtpType type, string code);
    bool Verify(Guid userId, OtpType type, string code, string hash);
}
public interface IEmailSender
{
    Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default);
}
public interface IPermissionProvider
{
    Task<IReadOnlyCollection<string>> GetPermissionsAsync(IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default);
    Task InvalidateAsync(Guid roleId, CancellationToken cancellationToken = default);
}

