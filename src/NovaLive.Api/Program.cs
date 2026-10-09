using System.Text.Json.Serialization;
using MediatR;
using NovaLive.Api.Auth;
using NovaLive.Api.Extensions;
using NovaLive.Api.Hubs;
using NovaLive.Api.Middleware;
using NovaLive.Application;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.Products;
using NovaLive.Infrastructure;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using NovaLive.Domain.Common;

var builder = WebApplication.CreateBuilder(args);

// 1. Structured Logging (Serilog)
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// 2. Core API, Context & Exception Handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

// 3. Auth & User Context
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ICurrentShop, CurrentShop>();
builder.Services.AddNovaLiveAuthentication(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Never trust arbitrary X-Forwarded-For headers; add only configured reverse proxies.
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
        options.KnownProxies.Add(System.Net.IPAddress.Parse(proxy));
});
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.OnRejected = async (context, ct) =>
    {
        var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? (int)Math.Ceiling(retryAfter.TotalSeconds) : 60;
        await AuthProblem.WriteAsync(context.HttpContext, new Error(ErrorType.TooManyRequests, "Auth.RateLimited", "Please try again later.")
            { RetryAfterSeconds = Math.Max(1, seconds) });
    };
});

// 4. CORS Policy (REST + WebSockets / SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

// 5. SignalR Realtime Service (With optional Redis Backplane)
var signalRBuilder = builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    signalRBuilder.AddStackExchangeRedis(redisConnectionString, redisOptions =>
    {
        redisOptions.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("NovaLive_SignalR");
    });
}

// 6. Application & Infrastructure Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 7. Controllers & OpenAPI
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

var app = builder.Build();
// Validate secrets before touching the database, rather than waiting for HostedService startup.
_ = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<NovaLive.Infrastructure.Auth.JwtOptions>>().Value;
_ = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<NovaLive.Infrastructure.Auth.OtpOptions>>().Value;

// Auto-run Database Migrations & Seeding on Startup
await app.ApplyMigrationsAsync();

// Middleware Pipeline
app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
// Handle auth exceptions before the request logger can serialize their messages.
app.UseExceptionHandler();
app.UseCors("DefaultCors");
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseMiddleware<JwtRoleContextMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

// ---- Controllers ----
app.MapControllers();

// ---- Fast Minimal APIs ----
var api = app.MapGroup("/api/v1");

api.MapGet("/products", async (
    string? keyword,
    Guid? shopId,
    Guid? categoryId,
    decimal? minPrice,
    decimal? maxPrice,
    string? sort,
    int page,
    int size,
    IProductQueryService productQueryService,
    CancellationToken cancellationToken) =>
{
    var request = new ProductSearchRequest(keyword, shopId, categoryId, minPrice, maxPrice, sort, page, size);
    var products = await productQueryService.SearchAsync(request, cancellationToken);
    return Results.Ok(ApiResponse<IReadOnlyList<ProductSummaryDto>>.Ok(products));
})
.WithName("SearchProducts");

// ---- SignalR Realtime Hub Endpoints ----
app.MapHub<LivestreamHub>("/hubs/livestream");
app.MapHub<OrderNotificationHub>("/hubs/order");
app.MapHub<PaymentNotificationHub>("/hubs/payment");

app.Run();
