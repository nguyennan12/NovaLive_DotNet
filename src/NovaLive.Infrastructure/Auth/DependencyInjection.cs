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
        services.AddOptions<EmailOptions>().Bind(configuration.GetSection("Email"))
            .Validate<IHostEnvironment>((options, environment) =>
                options.UseLogging(environment) || options.IsResend,
                "Email:Provider must be Resend; Logging is available only in Development.")
            .Validate<IHostEnvironment>((options, environment) => options.UseLogging(environment)
                || (!string.IsNullOrWhiteSpace(options.ApiKey) && !string.IsNullOrWhiteSpace(options.FromAddress)),
                "Email:ApiKey and Email:FromAddress are required for Resend.")
            .ValidateOnStart();
        services.AddTransient<LoggingEmailSender>();
        services.AddHttpClient<ResendEmailSender>(client => client.Timeout = TimeSpan.FromSeconds(10))
            // Disable framework HTTP logging too: headers and provider exceptions can contain secrets.
            .RemoveAllLoggers()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IOtpService, OtpService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddTransient<IEmailSender>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<EmailOptions>>().Value;
            var environment = provider.GetRequiredService<IHostEnvironment>();
            if (settings.UseLogging(environment))
                return provider.GetRequiredService<LoggingEmailSender>();
            return provider.GetRequiredService<ResendEmailSender>();
        });
        return services;
    }
}
