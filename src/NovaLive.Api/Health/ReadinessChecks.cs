using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NovaLive.Domain.Rbac;
using NovaLive.Infrastructure.Persistence;
using StackExchange.Redis;

namespace NovaLive.Api.Health;

public sealed class DatabaseHealthCheck(AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => await db.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Database is unavailable.");
}

public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!redis.IsConnected)
            {
                return HealthCheckResult.Unhealthy("Redis is unavailable.");
            }
            await redis.GetDatabase().PingAsync().WaitAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unavailable: " + ex.Message);
        }
    }
}

public sealed class SystemRolesHealthCheck(AppDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var expected = new Dictionary<Guid, string>
        {
            [SystemRoleIds.Admin] = "Admin", [SystemRoleIds.Seller] = "Seller", [SystemRoleIds.Buyer] = "Buyer"
        };
        var ids = expected.Keys.ToArray();
        var roles = await db.Roles.AsNoTracking().Where(role => ids.Contains(role.Id) && role.IsSystem)
            .Select(role => new { role.Id, role.Name }).ToListAsync(cancellationToken);
        var data = expected.ToDictionary(role => role.Value,
            role => (object)roles.Any(seeded => seeded.Id == role.Key && seeded.Name == role.Value));
        return data.Values.All(value => (bool)value)
            ? HealthCheckResult.Healthy(data: data)
            : HealthCheckResult.Unhealthy("Required system roles are missing.", data: data);
    }
}
