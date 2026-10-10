using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NovaLive.Contracts.Common;
using NovaLive.Domain.Common;
using NovaLive.Api.Auth;

namespace NovaLive.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddNovaLiveAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:Secret"] ?? "";
        if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
            throw new InvalidOperationException("Jwt:Secret must contain at least 32 UTF-8 bytes.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience are required.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.SaveToken = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = "role",
                NameClaimType = "sub",
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Support WebSocket / SignalR Token Handshake via Query Parameter
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    await AuthProblem.WriteAsync(context.HttpContext,
                        Error.Unauthorized("Auth.Unauthorized", "Please sign in."));
                },
                OnForbidden = async context =>
                {
                    await AuthProblem.WriteAsync(context.HttpContext,
                        Error.Forbidden("Auth.Forbidden", "Access denied."));
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
