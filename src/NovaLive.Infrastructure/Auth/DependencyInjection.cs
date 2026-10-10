using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Auth;
namespace NovaLive.Infrastructure.Auth;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>().Bind(configuration.GetSection("Jwt"))
            .Validate(options => options.IsValid(), "Jwt:Secret must contain at least 32 UTF-8 bytes; Issuer and Audience are required.")
            .ValidateOnStart();
        services.AddOptions<OtpOptions>().Bind(configuration.GetSection("Otp"))
            .Validate(options => Encoding.UTF8.GetByteCount(options.Pepper) >= 32, "Otp:Pepper must contain at least 32 UTF-8 bytes.")
            .ValidateOnStart();
        services.AddOptions<SmtpOptions>().Bind(configuration.GetSection("Smtp"));
        services.AddOptions<GoogleOptions>().Configure(options =>
        {
            options.Enabled = configuration.GetValue<bool>("Auth:Google:Enabled");
            options.ClientId = configuration["Google:ClientId"] ?? "";
        }).Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.ClientId),
            "Google:ClientId is required when Google login is enabled.").ValidateOnStart();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IOtpService, OtpService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddTransient<IEmailSender>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<SmtpOptions>>().Value;
            var environment = provider.GetRequiredService<IHostEnvironment>();
            if (settings.UseLoggingSender && environment.IsDevelopment())
                return ActivatorUtilities.CreateInstance<LoggingEmailSender>(provider);
            return ActivatorUtilities.CreateInstance<SmtpEmailSender>(provider);
        });
        return services;
    }
}
