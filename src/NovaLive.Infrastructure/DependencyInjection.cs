using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Infrastructure.Auth;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Interceptors;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Persistence.Seeding;
using NovaLive.Infrastructure.Services;
using NovaLive.Infrastructure.System;
using StackExchange.Redis;

namespace NovaLive.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthServices(configuration);
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=novalive_db;Username=nova_user;Password=nova_password";

        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<DomainEventsDispatcherInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();

            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
                serviceProvider.GetRequiredService<SoftDeleteInterceptor>(),
                serviceProvider.GetRequiredService<DomainEventsDispatcherInterceptor>());
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IAuthPersistence, AuthPersistence>();
        services.AddScoped<IAfterCommitActions, AfterCommitActions>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IProductQueryService, PostgresProductQueryService>();

        // Repositories
        services.AddScoped<ISpuRepository, NovaLive.Infrastructure.Persistence.Repositories.SpuRepository>();
        services.AddScoped<ISkuRepository, NovaLive.Infrastructure.Persistence.Repositories.SkuRepository>();
        services.AddScoped<IInventoryRepository, NovaLive.Infrastructure.Persistence.Repositories.InventoryRepository>();
        services.AddScoped<ICategoryRepository, NovaLive.Infrastructure.Persistence.Repositories.CategoryRepository>();

        // Migration & Seeding
        services.AddScoped<IDataSeeder, RbacDataSeeder>();
        services.AddScoped<IDataSeeder, CategoryDataSeeder>();
        services.AddScoped<IMigrationService, MigrationService>();

        // Caching & Idempotency
        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");
        if (string.IsNullOrWhiteSpace(redisConnectionString))
            throw new InvalidOperationException("Redis:ConnectionString is required for authentication and revocation checks.");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddSingleton<IIdempotencyService, RedisIdempotencyService>();
        }

        // Outbox background worker
        services.AddHostedService<OutboxBackgroundWorker>();

        // Messaging
        services.AddMassTransit(bus =>
        {
            bus.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
                cfg.Host(host, "/", h =>
                {
                    h.Username(configuration.GetValue<string>("RabbitMQ:Username") ?? "guest");
                    h.Password(configuration.GetValue<string>("RabbitMQ:Password") ?? "guest");
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<IMessageBus, MassTransitMessageBus>();

        return services;
    }
}
