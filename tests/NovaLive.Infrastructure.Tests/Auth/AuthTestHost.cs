using System.Collections.Concurrent;
using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NovaLive.Application;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Auth;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Repositories;
using Npgsql;
namespace NovaLive.Infrastructure.Tests.Auth;

internal sealed class AuthTestHost : IAsyncDisposable
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private string? postgresDatabase;
    private string? postgresAdminConnection;
    public ServiceProvider Services { get; private set; } = null!;
    public TestClock Clock { get; } = new();
    public TestCache Cache { get; }
    public Mock<IEmailSender> Mail { get; } = new();
    public Mock<IPasswordHasher> Passwords { get; } = new();
    public Mock<ICurrentUser> CurrentUser { get; } = new();
    public ConcurrentDictionary<string, string> Codes { get; } = new();
    public AuthTestHost() { Cache = new(Clock); }
    public async Task InitializeAsync()
    {
        postgresAdminConnection = Environment.GetEnvironmentVariable("NOVALIVE_TEST_POSTGRES");
        string? postgresConnection = null;
        if (string.IsNullOrWhiteSpace(postgresAdminConnection)) await connection.OpenAsync();
        else
        {
            postgresDatabase = "novalive_auth_test_" + Guid.NewGuid().ToString("N");
            await using var admin = new NpgsqlConnection(postgresAdminConnection);
            await admin.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE {postgresDatabase}", admin);
            await command.ExecuteNonQueryAsync();
            postgresConnection = new NpgsqlConnectionStringBuilder(postgresAdminConnection) { Database = postgresDatabase, Pooling = false }.ConnectionString;
        }
        Passwords.Setup(p => p.Hash(It.IsAny<string>())).Returns((string p) => "hash:" + p);
        Passwords.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string p, string h) => h == "hash:" + p);
        Mail.Setup(m => m.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OtpType>(), It.IsAny<CancellationToken>()))
            .Callback((string email, string code, OtpType type, CancellationToken ct) => Codes[email] = code)
            .Returns(Task.CompletedTask);
        CurrentUser.SetupGet(x => x.Roles).Returns(["Buyer"]);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<AppDbContext>(options =>
        {
            if (postgresConnection is null)
                options.UseSqlite(connection).ReplaceService<IModelCustomizer, SqliteTimeModelCustomizer>();
            else options.UseNpgsql(postgresConnection);
            options.UseSnakeCaseNamingConvention();
        });
        services.AddScoped<IAppDbContext>(p => p.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<AppDbContext>());
        services.AddScoped<IAuthPersistence, AuthPersistence>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IAfterCommitActions, AfterCommitActions>();
        services.AddScoped<ISpuRepository, SpuRepository>();
        services.AddScoped<ISkuRepository, SkuRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IDateTimeProvider>(Clock);
        services.AddSingleton<ICacheService>(Cache);
        services.AddSingleton(CurrentUser.Object);
        services.AddSingleton(Passwords.Object);
        services.AddSingleton(Mail.Object);
        services.AddSingleton<IIdempotencyService>(Mock.Of<IIdempotencyService>());
        services.AddScoped<IPermissionProvider, PermissionProvider>();
        services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IOtpService>(new OtpService(Options.Create(new OtpOptions { Pepper = new string('p', 32) })));
        services.AddSingleton<IJwtTokenService>(new JwtTokenService(Options.Create(new JwtOptions
            { Secret = new string('s', 32), Issuer = "tests", Audience = "tests" }), Clock));
        Services = services.BuildServiceProvider();
        await WithDb(async db =>
        {
            if (postgresConnection is null) await db.Database.EnsureCreatedAsync();
            else await db.Database.MigrateAsync();
            db.Roles.Add(new Role(SystemRoleIds.Buyer, "Buyer", Clock.UtcNow, isSystem: true));
            await db.SaveChangesAsync();
        });
    }
    public async Task<T> Send<T>(IRequest<T> request)
    {
        using var scope = Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }
    public async Task WithDb(Func<AppDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
    public async Task<Guid> AddUser(string email = "buyer@example.com", bool active = true, string phone = "0912345678")
    {
        var user = new User(email, "hash:Password123", "Test User", phone);
        if (active) user.Activate();
        await WithDb(async db =>
        {
            db.Users.Add(user);
            db.UserRoles.Add(new UserRole(user.Id, SystemRoleIds.Buyer, Clock.UtcNow));
            await db.SaveChangesAsync();
        });
        return user.Id;
    }
    public void Authenticate(Guid id)
    {
        CurrentUser.SetupGet(x => x.UserId).Returns(id);
        CurrentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        CurrentUser.SetupGet(x => x.Jti).Returns(Guid.NewGuid().ToString());
        CurrentUser.SetupGet(x => x.AccessTokenExpiresAt).Returns(Clock.UtcNow.AddMinutes(12));
    }
    public async ValueTask DisposeAsync()
    {
        if (Services is not null) await Services.DisposeAsync();
        await connection.DisposeAsync();
        if (postgresDatabase is not null)
        {
            // Only remove the uniquely named database created by this fixture.
            await using var admin = new NpgsqlConnection(postgresAdminConnection);
            await admin.OpenAsync();
            await using var command = new NpgsqlCommand($"DROP DATABASE {postgresDatabase} WITH (FORCE)", admin);
            await command.ExecuteNonQueryAsync();
        }
    }
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
internal sealed class TestClock : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);
}
internal sealed class TestCache(TestClock clock) : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTimeOffset? Expires)> values = [];
    private readonly object gate = new();
    private object? Read(string key)
    {
        if (!values.TryGetValue(key, out var value)) return null;
        if (value.Expires <= clock.UtcNow) { values.Remove(key); return null; }
        return value.Value;
    }
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        lock (gate) return Task.FromResult(Read(key) is T value ? value : default);
    }
    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        lock (gate) values[key] = (value!, ttl is null ? null : clock.UtcNow + ttl);
        return Task.CompletedTask;
    }
    public async Task<T> GetOrSetAsync<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null) return cached;
        var value = await factory(cancellationToken);
        await SetAsync(key, value, ttl, cancellationToken);
        return value;
    }
    public Task<long> IncrementAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            var previous = Read(key);
            var count = previous is long number ? number + 1 : 1;
            values[key] = (count, previous is null ? clock.UtcNow + ttl : values[key].Expires);
            return Task.FromResult(count);
        }
    }
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (gate) return Task.FromResult(Read(key) is not null);
    }
    public Task<TimeSpan?> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (gate) return Task.FromResult(Read(key) is null ? null : values[key].Expires - clock.UtcNow);
    }
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (gate) values.Remove(key);
        return Task.CompletedTask;
    }
    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        lock (gate)
            foreach (var key in values.Keys.Where(key => key.StartsWith(prefixKey)).ToArray()) values.Remove(key);
        return Task.CompletedTask;
    }
}

