using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Seeding;
using StackExchange.Redis;

namespace NovaLive.Api.Tests.Auth;

internal sealed class AuthApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string environment;
    private readonly Dictionary<string, string?> overrides;
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    public CapturingEmailSender Mail { get; } = new();
    public MutableClock Clock { get; } = new();
    public Mock<IDatabase> RedisDatabase { get; } = new();
    public TaskCompletionSource InitializationEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource ReleaseInitialization { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public bool InitializationComplete { get; private set; }
    public const string TestApiKey = "fake-resend-key-must-not-appear-in-health";

    public AuthApplicationFactory(bool pauseInitialization = false, string environment = "Testing", Dictionary<string, string?>? settings = null)
    {
        this.environment = environment;
        overrides = settings ?? [];
        connection.Open();
        RedisDatabase.Setup(db => db.PingAsync(It.IsAny<CommandFlags>())).ReturnsAsync(TimeSpan.FromMilliseconds(1));
        if (!pauseInitialization) ReleaseInitialization.SetResult();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Authentication reads configuration before builder.Build; pass test settings to the entry point too.
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(TestConfiguration).AddInMemoryCollection(overrides));
        return base.CreateHost(builder);
    }

    private static Dictionary<string, string?> TestConfiguration => new()
    {
        ["Jwt:Secret"] = new string('s', 32), ["Jwt:Issuer"] = "tests", ["Jwt:Audience"] = "tests",
        ["Otp:Pepper"] = new string('p', 32), ["Email:Provider"] = "Both",
        ["Email:ApiKey"] = TestApiKey, ["Email:FromAddress"] = "test@example.com", ["Email:LogOtp"] = "false",
        ["Serilog:MinimumLevel:Default"] = "Fatal",
        ["Serilog:MinimumLevel:Override:Microsoft.AspNetCore"] = "Fatal",
        ["Redis:ConnectionString"] = "test-redis:6379",
        ["Minio:AccessKey"] = "test-access-key",
        ["Minio:SecretKey"] = "test-secret-key"
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(TestConfiguration).AddInMemoryCollection(overrides));
        builder.ConfigureTestServices(services =>
        {
            // Run the real Program, DI, pipeline and handlers, isolating external infrastructure.
            services.RemoveAll<IHostedService>();
            services.PostConfigure<HealthCheckServiceOptions>(options =>
            {
                // The real RabbitMQ bus is intentionally not started in this fixture.
                foreach (var check in options.Registrations.Where(check => check.Name == "masstransit-bus").ToArray())
                    options.Registrations.Remove(check);
            });
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection)
                .UseSnakeCaseNamingConvention().ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>());
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Mail);
            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton<IDateTimeProvider>(Clock);
            services.RemoveAll<IConnectionMultiplexer>();
            var redis = new Mock<IConnectionMultiplexer>();
            redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(RedisDatabase.Object);
            services.AddSingleton(redis.Object);
            services.RemoveAll<ICacheService>();
            services.AddSingleton(CreateCache());
            services.RemoveAll<IIdempotencyService>();
            services.AddSingleton(Mock.Of<IIdempotencyService>());
            services.RemoveAll<IMigrationService>();
            services.AddScoped<IMigrationService>(provider => new SqliteInitialization(provider.GetRequiredService<AppDbContext>(),
                provider.GetServices<IDataSeeder>(), this));
        });
    }

    private ICacheService CreateCache()
    {
        var counts = new ConcurrentDictionary<string, long>();
        var flags = new ConcurrentDictionary<string, (bool Value, DateTimeOffset? Expires)>();
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns((string key, bool value, TimeSpan? ttl, CancellationToken ct) =>
            { flags[key] = (value, ttl.HasValue ? Clock.UtcNow + ttl.Value : null); return Task.CompletedTask; });
        cache.Setup(c => c.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string key, CancellationToken ct) => Task.FromResult(flags.TryGetValue(key, out var entry)
                && (entry.Expires is null || entry.Expires > Clock.UtcNow)));
        cache.Setup(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<CancellationToken, Task<string[]>>>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, Task<string[]>> factory, TimeSpan? ttl, CancellationToken ct) => factory(ct));
        cache.Setup(c => c.GetAsync<long>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string key, CancellationToken _) => Task.FromResult(counts.GetValueOrDefault(key)));
        cache.Setup(c => c.IncrementAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns((string key, TimeSpan _, CancellationToken _) => Task.FromResult(counts.AddOrUpdate(key, 1, (_, count) => count + 1)));
        cache.Setup(c => c.GetTimeToLiveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string key, CancellationToken ct) => Task.FromResult(flags.TryGetValue(key, out var entry)
                ? entry.Expires - Clock.UtcNow : (TimeSpan?)TimeSpan.FromMinutes(5)));
        cache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns((string key, CancellationToken ct) => { counts.TryRemove(key, out _); flags.TryRemove(key, out _); return Task.CompletedTask; });
        return cache.Object;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) connection.Dispose();
    }

    private sealed class SqliteInitialization(AppDbContext db, IEnumerable<IDataSeeder> seeders, AuthApplicationFactory owner) : IMigrationService
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            owner.InitializationEntered.TrySetResult();
            await owner.ReleaseInitialization.Task.WaitAsync(cancellationToken);
            // PostgreSQL migrations cannot run on SQLite; exercise the existing real seeders.
            await db.Database.EnsureCreatedAsync(cancellationToken);
            foreach (var seeder in seeders.OrderBy(seeder => seeder.Order)) await seeder.SeedAsync(cancellationToken);
            owner.InitializationComplete = true;
        }
    }
}

internal sealed class CapturingEmailSender : IEmailSender
{
    public ConcurrentQueue<(string Email, string Code, OtpType Type)> Deliveries { get; } = new();
    public Task SendOtpAsync(string email, string otp, OtpType type, CancellationToken cancellationToken = default)
    {
        Deliveries.Enqueue((email, otp, type));
        return Task.CompletedTask;
    }
}

internal sealed class MutableClock : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class SqliteTimeModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entity.GetProperties())
            if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
    }
}
