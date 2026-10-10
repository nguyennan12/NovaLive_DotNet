using NovaLive.Api.Extensions;
using NovaLive.Application;
using NovaLive.Infrastructure;
using Serilog;

// ---------- 1. Bootstrap Logger ----------
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------- 2. Structured Logging (Serilog Host) ----------
    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });

    // ---------- 3. Dependency Injection Modules ----------
    builder.Services.AddApiServices(builder.Configuration, builder.Environment);
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // ---------- 4. Early Option Validation ----------
    _ = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<NovaLive.Infrastructure.Auth.JwtOptions>>().Value;
    _ = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<NovaLive.Infrastructure.Auth.OtpOptions>>().Value;

    // ---------- 5. Auto-run Database Migrations & Multi-part Seeders ----------
    await app.ApplyMigrationsAsync();

    // ---------- 6. Middleware & Endpoints Pipeline ----------
    app.UseApiPipeline();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NovaLive.Api terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}
