using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NovaLive.Contracts.Common;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddNovaLiveAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSecret = configuration["Jwt:Secret"]
            ?? configuration["Jwt:SecretKey"]
            ?? "NovaLive_Super_Secret_Jwt_Key_For_Development_Environment_2026_KeyMustBeLongEnough!";
        var issuer = configuration["Jwt:Issuer"] ?? "NovaLive";
        var audience = configuration["Jwt:Audience"] ?? "NovaLiveClients";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
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
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.Fail(new ApiError
                    {
                        Code = "AUTH_UNAUTHORIZED",
                        Message = "Vui lòng đăng nhập để tiếp tục.",
                        Type = ErrorType.Unauthorized
                    });

                    await context.Response.WriteAsJsonAsync(response);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.Fail(new ApiError
                    {
                        Code = "AUTH_FORBIDDEN",
                        Message = "Bạn không có quyền truy cập tài nguyên này.",
                        Type = ErrorType.Forbidden
                    });

                    await context.Response.WriteAsJsonAsync(response);
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
