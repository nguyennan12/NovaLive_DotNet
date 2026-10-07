using MediatR;
using NovaLive.Api.Auth;
using NovaLive.Api.Extensions;
using NovaLive.Api.Hubs;
using NovaLive.Api.Middleware;
using NovaLive.Application;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Application.System.Queries;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.Products;
using NovaLive.Infrastructure;
using NovaLive.Infrastructure.Persistence.Seeding;
using Serilog;

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

// 7. API Documentation (OpenAPI)
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-run Database Migrations & Seeding on Startup
await app.ApplyMigrationsAsync();

// Middleware Pipeline
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("DefaultCors");
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// ---- REST API Endpoints ----
var api = app.MapGroup("/api/v1");

api.MapGet("/health/live", () => Results.Ok(ApiResponse<object>.Ok(new { status = "Live", timestamp = DateTimeOffset.UtcNow })))
    .WithName("HealthLive");

api.MapGet("/health/ready", async (ISender sender, CancellationToken cancellationToken) =>
{
    var status = await sender.Send(new GetSystemStatusQuery(), cancellationToken);
    return Results.Ok(ApiResponse<SystemStatusDto>.Ok(status));
})
.WithName("HealthReady");

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
