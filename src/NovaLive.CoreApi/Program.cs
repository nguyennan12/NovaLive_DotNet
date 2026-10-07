using MediatR;
using NovaLive.Application;
using NovaLive.Application.Abstractions.CurrentUser;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Application.System.Queries;
using NovaLive.Contracts.Products;
using NovaLive.CoreApi.Security;
using NovaLive.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console();
});

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddNovaLiveAuthentication(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api/v1");

api.MapGet("/health", async (ISender sender, CancellationToken cancellationToken) =>
{
    return Results.Ok(await sender.Send(new GetSystemStatusQuery(), cancellationToken));
})
.WithName("GetHealth");

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
    return Results.Ok(products);
})
.WithName("SearchProducts");

app.Run();
