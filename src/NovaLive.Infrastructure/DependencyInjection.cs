using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Abstractions.Cache;
using NovaLive.Application.Abstractions.Clock;
using NovaLive.Application.Abstractions.Idempotency;
using NovaLive.Application.Abstractions.Messaging;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Search;
using NovaLive.Infrastructure.Cache;
using NovaLive.Infrastructure.Clock;
using NovaLive.Infrastructure.Idempotency;
using NovaLive.Infrastructure.Messaging;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Interceptors;
using NovaLive.Infrastructure.Persistence.Seeding;
using NovaLive.Infrastructure.Search;
using NovaLive.Infrastructure.System;
using StackExchange.Redis;

namespace NovaLive.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
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
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IProductQueryService, PostgresProductQueryService>();

        // Migration & Seeding
        services.AddScoped<IDataSeeder, SystemDataSeeder>();
        services.AddScoped<IMigrationService, MigrationService>();

        // Caching & Idempotency
        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");
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
