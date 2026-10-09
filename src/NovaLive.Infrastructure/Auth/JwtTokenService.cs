using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Clock;

namespace NovaLive.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options, IDateTimeProvider clock) : IJwtTokenService
{
    public AccessToken Create(Guid userId, string email, IReadOnlyCollection<string> roles, Guid? shopId)
    {
        var now = clock.UtcNow;
        var expiresAt = now.AddMinutes(15);
        var jti = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new("sub", userId.ToString()), new("jti", jti), new("email", email),
            new("iat", now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        claims.AddRange(roles.Distinct().Select(role => new Claim("role", role)));
        if (shopId.HasValue) claims.Add(new("shopId", shopId.Value.ToString()));
        var settings = options.Value;
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            now.UtcDateTime, expiresAt.UtcDateTime, new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)), SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), jti, expiresAt);
    }
}

