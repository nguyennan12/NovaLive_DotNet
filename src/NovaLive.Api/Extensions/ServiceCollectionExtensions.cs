using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using NovaLive.Api.Auth;
using NovaLive.Api.Middleware;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // 1. Core API, Context & Exception Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddHttpContextAccessor();

        // 2. Auth Context & Current Identity
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<ICurrentShop, CurrentShop>();
        services.AddNovaLiveAuthentication(configuration);

        // 3. Security & Rate Limiting
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var proxy in configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
            {
                options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
            }
        });

        services.AddRateLimiter(options =>
        {
            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

            options.OnRejected = async (context, _) =>
            {
                var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)Math.Ceiling(retryAfter.TotalSeconds)
                    : 60;

                await AuthProblem.WriteAsync(
                    context.HttpContext,
                    new Error(ErrorType.TooManyRequests, "Auth.RateLimited", "Vui lòng thử lại sau ít phút.")
                    {
                        RetryAfterSeconds = Math.Max(1, seconds)
                    });
            };
        });

        // 4. CORS Policy
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                if (origins.Any(origin => !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                    || uri.Scheme is not ("http" or "https") || origin.Contains('*')
                    || uri.GetLeftPart(UriPartial.Authority) != origin))
                    throw new InvalidOperationException("Cors:AllowedOrigins must contain explicit HTTP(S) origins without wildcards or paths.");
                policy.WithOrigins(origins).AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // 5. SignalR Realtime Hubs
        var signalRBuilder = services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = environment.IsDevelopment();
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            var signalRRedisOptions = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
            signalRRedisOptions.AbortOnConnectFail = false;
            signalRRedisOptions.ConnectRetry = 5;
            signalRRedisOptions.ConnectTimeout = 5000;
            signalRRedisOptions.SyncTimeout = 5000;

            signalRBuilder.AddStackExchangeRedis(options =>
            {
                options.Configuration = signalRRedisOptions;
                options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("NovaLive_SignalR");
            });
        }

        services.AddScoped<NovaLive.Application.Abstractions.Services.ILivestreamNotificationService, NovaLive.Api.Services.LivestreamNotificationService>();
        services.AddSingleton<NovaLive.Api.Services.LivestreamReactionBatcher>();
        services.AddSingleton<NovaLive.Api.Services.ILivestreamReactionBatcher>(sp => sp.GetRequiredService<NovaLive.Api.Services.LivestreamReactionBatcher>());
        services.AddHostedService(sp => sp.GetRequiredService<NovaLive.Api.Services.LivestreamReactionBatcher>());

        // 6. Controllers & JSON Options
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // 7. OpenAPI Documentation & Scalar
        services.AddOpenApiDocumentation();

        // 8. Health Checks
        services.AddHealthChecks()
            .AddCheck<NovaLive.Api.Health.DatabaseHealthCheck>("database", timeout: TimeSpan.FromSeconds(5))
            .AddCheck<NovaLive.Api.Health.RedisHealthCheck>("redis", timeout: TimeSpan.FromSeconds(5))
            .AddCheck<NovaLive.Api.Health.SystemRolesHealthCheck>("systemRoles", timeout: TimeSpan.FromSeconds(5));

        return services;
    }
}
