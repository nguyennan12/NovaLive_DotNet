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
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true);
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
            signalRBuilder.AddStackExchangeRedis(redisConnectionString, redisOptions =>
            {
                redisOptions.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("NovaLive_SignalR");
            });
        }

        // 6. Controllers & JSON Options
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // 7. OpenAPI Documentation
        services.AddOpenApi();

        // 8. Health Checks
        services.AddHealthChecks();

        return services;
    }
}
