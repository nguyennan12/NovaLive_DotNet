using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            .Validate(options => options.IsLogging || options.IsResend || options.IsBoth,
                "Email:Provider must be one of Logging, Resend, Both.")
            .Validate(options => !options.RequiresResend
                || (!string.IsNullOrWhiteSpace(options.ApiKey) && !string.IsNullOrWhiteSpace(options.FromAddress)),
                "Email:ApiKey and Email:FromAddress are required for Resend or Both.")
            .ValidateOnStart();
        services.AddTransient<LoggingEmailSender>();
        services.AddHttpClient<ResendEmailSender>(client => client.Timeout = TimeSpan.FromSeconds(10))
            // Disable framework HTTP logging too: headers and provider exceptions can contain secrets.
            .RemoveAllLoggers()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddTransient(provider => new CompositeEmailSender(
            [provider.GetRequiredService<LoggingEmailSender>(), provider.GetRequiredService<ResendEmailSender>()],
            provider.GetRequiredService<ILogger<CompositeEmailSender>>()));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IOtpService, OtpService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddTransient<IEmailSender>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<EmailOptions>>().Value;
            if (settings.IsLogging)
                return provider.GetRequiredService<LoggingEmailSender>();
            if (settings.IsBoth)
                return provider.GetRequiredService<CompositeEmailSender>();
            return provider.GetRequiredService<ResendEmailSender>();
        });
        return services;
    }
}
