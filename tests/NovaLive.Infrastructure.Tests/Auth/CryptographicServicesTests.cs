using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Auth;
namespace NovaLive.Infrastructure.Tests.Auth;

public sealed class CryptographicServicesTests
{
    [Fact]
    public void Passwords_UseBcryptCost12()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Testing123!");
        hash.Should().StartWith("$2a$12$");
        hasher.Verify("Testing123!", hash).Should().BeTrue();
        hasher.Verify("wrong", hash).Should().BeFalse();
    }
    [Fact]
    public void Otp_IsSixDigitsAndBoundToUserTypeAndPepper()
    {
        var service = new OtpService(Options.Create(new OtpOptions { Pepper = new string('p', 32) }));
        var userId = Guid.NewGuid();
        var code = service.Generate();
        code.Should().MatchRegex("^[0-9]{6}$");
        var hash = service.Hash(userId, OtpType.EmailVerify, code);
        hash.Should().NotBe(code).And.HaveLength(64);
        service.Verify(userId, OtpType.EmailVerify, code, hash).Should().BeTrue();
        service.Verify(userId, OtpType.ForgotPassword, code, hash).Should().BeFalse();
        service.Verify(Guid.NewGuid(), OtpType.EmailVerify, code, hash).Should().BeFalse();
    }
    [Fact]
    public void RefreshToken_IsRandom64BytesAndSha256()
    {
        var service = new RefreshTokenService();
        var token = service.Create();
        Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(token.Value).Should().HaveCount(64);
        token.Hash.Should().HaveLength(64).And.Be(service.Hash(token.Value)).And.NotBe(token.Value);
        service.Create().Value.Should().NotBe(token.Value);
    }
    [Fact]
    public void Jwt_ContainsRequiredClaimsAndExpiresIn15Minutes()
    {
        var now = new DateTimeOffset(2026, 10, 8, 0, 0, 0, TimeSpan.Zero);
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Secret = new string('s', 32), Issuer = "tests", Audience = "tests"
        }), new FixedClock(now));
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var token = service.Create(userId, "test@example.com", ["Buyer", "Seller"], shopId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);
        jwt.Header.Alg.Should().Be("HS256");
        jwt.Subject.Should().Be(userId.ToString());
        jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).Should().BeEquivalentTo("Buyer", "Seller");
        jwt.Claims.Single(c => c.Type == "shopId").Value.Should().Be(shopId.ToString());
        jwt.Claims.Single(c => c.Type == "email").Value.Should().Be("test@example.com");
        Guid.TryParse(jwt.Id, out _).Should().BeTrue();
        token.Jti.Should().Be(jwt.Id);
        token.ExpiresAt.Should().Be(now.AddMinutes(15));
        jwt.ValidTo.Should().Be(now.AddMinutes(15).UtcDateTime);
    }
    private sealed class FixedClock(DateTimeOffset now) : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => now;
    }
}
