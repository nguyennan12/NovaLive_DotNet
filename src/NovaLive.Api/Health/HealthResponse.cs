using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NovaLive.Infrastructure.Auth;

namespace NovaLive.Api.Health;

public static class HealthResponse
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
        => context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            emailProvider = context.RequestServices.GetRequiredService<IOptions<EmailOptions>>().Value.Provider,
            checks = report.Entries.ToDictionary(entry => entry.Key, entry => new
            {
                status = entry.Value.Status.ToString(),
                data = entry.Key == "systemRoles" ? entry.Value.Data : new Dictionary<string, object>()
            })
            // Only system-role booleans are public data. Do not expose third-party check diagnostics.
        }, cancellationToken: context.RequestAborted);
}
