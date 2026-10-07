using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace NovaLive.CoreApi.Security;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddNovaLiveAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration.GetValue<string>("Jwt:SecretKey")
            ?? "development-secret-key-change-me-development-secret-key";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization();
        return services;
    }
}
