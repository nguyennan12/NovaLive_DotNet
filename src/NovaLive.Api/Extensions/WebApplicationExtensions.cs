using NovaLive.Api.Hubs;
using NovaLive.Api.Middleware;
using Serilog;

namespace NovaLive.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        // 1. Exception Handling (Top of pipeline to catch all downstream errors)
        app.UseExceptionHandler();

        // 2. Network & Security Headers
        app.UseForwardedHeaders();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // 3. Structured Request Logging
        app.UseSerilogRequestLogging();

        // 4. CORS & Protocol
        app.UseCors("DefaultCors");
        app.UseHttpsRedirection();

        // 5. OpenAPI in Development
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // 6. Authentication & RBAC Context
        app.UseAuthentication();
        app.UseMiddleware<JwtRoleContextMiddleware>();
        app.UseAuthorization();
        app.UseRateLimiter();

        // 7. REST Controllers
        app.MapControllers();

        // 8. Realtime SignalR Hubs
        app.MapHub<LivestreamHub>("/hubs/livestream");
        app.MapHub<OrderNotificationHub>("/hubs/order");
        app.MapHub<PaymentNotificationHub>("/hubs/payment");

        // 9. Health Checks
        app.MapHealthChecks("/health");

        return app;
    }
}
