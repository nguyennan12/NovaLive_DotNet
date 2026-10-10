using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private static readonly Lazy<string> DummyHash = new(() =>
        BCrypt.Net.BCrypt.HashPassword(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)), 12));
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, 12);
    public bool Verify(string password, string hash) =>
        !string.IsNullOrEmpty(hash) && BCrypt.Net.BCrypt.Verify(password, hash);
    public void VerifyDummy(string password) => BCrypt.Net.BCrypt.Verify(password, DummyHash.Value);
}
public sealed class RefreshTokenService : IRefreshTokenService
{
    public RefreshTokenValue Create()
    {
        var value = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        return new(value, Hash(value));
    }
    public string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
public sealed class OtpService(IOptions<OtpOptions> options) : IOtpService
{
    public string Generate() => RandomNumberGenerator.GetInt32(1_000_000).ToString("D6", CultureInfo.InvariantCulture);
    public string Hash(Guid userId, OtpType type, string code) =>
        Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(options.Value.Pepper),
            Encoding.UTF8.GetBytes($"{userId:D}:{type}:{code}")));
    public bool Verify(Guid userId, OtpType type, string code, string hash)
    {
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(Hash(userId, type, code)), Convert.FromHexString(hash));
        }
        catch (FormatException) { return false; }
    }
}

