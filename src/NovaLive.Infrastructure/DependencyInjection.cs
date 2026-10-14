using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Infrastructure.Auth;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Interceptors;
using NovaLive.Infrastructure.Persistence.Repositories;
using NovaLive.Infrastructure.Persistence.Seeding;
using NovaLive.Infrastructure.Services;
using NovaLive.Infrastructure.System;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Infrastructure.Shipping.Ghn;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace NovaLive.Infrastructure;


public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthServices(configuration);
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

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
        services.AddScoped<IShopRepository, ShopRepository>();
        services.Configure<NovaLive.Application.UseCases.Shops.KycUrlOptions>(
            configuration.GetSection(NovaLive.Application.UseCases.Shops.KycUrlOptions.SectionName));
        services.AddScoped<IAfterCommitActions, AfterCommitActions>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IProductQueryService, PostgresProductQueryService>();

        // Repositories
        services.AddScoped<IUserRepository, NovaLive.Infrastructure.Persistence.Repositories.UserRepository>();
        services.AddScoped<IPermissionRepository, NovaLive.Infrastructure.Persistence.Repositories.PermissionRepository>();
        services.AddScoped<ISpuRepository, NovaLive.Infrastructure.Persistence.Repositories.SpuRepository>();
        services.AddScoped<ISkuRepository, NovaLive.Infrastructure.Persistence.Repositories.SkuRepository>();
        services.AddScoped<IInventoryRepository, NovaLive.Infrastructure.Persistence.Repositories.InventoryRepository>();
        services.AddScoped<IDiscountRepository, NovaLive.Infrastructure.Persistence.Repositories.DiscountRepository>();
        services.AddScoped<ICartRepository, NovaLive.Infrastructure.Persistence.Repositories.CartRepository>();
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
                var host = configuration.GetValue<string>("RabbitMQ:Host")
                    ?? throw new InvalidOperationException("RabbitMQ:Host is required.");
                var username = configuration.GetValue<string>("RabbitMQ:Username")
                    ?? throw new InvalidOperationException("RabbitMQ:Username is required.");
                var password = configuration.GetValue<string>("RabbitMQ:Password")
                    ?? throw new InvalidOperationException("RabbitMQ:Password is required.");
                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddScoped<IMessageBus, MassTransitMessageBus>();

        // GHN Shipping Provider
        services.Configure<GhnOptions>(configuration.GetSection(GhnOptions.SectionName));
        services.AddSingleton<IGhnChecksumValidator, GhnChecksumValidator>();
        services.AddHttpClient<IShippingProvider, GhnShippingAdapter>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<GhnOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            if (!string.IsNullOrWhiteSpace(options.ApiToken))
            {
                client.DefaultRequestHeaders.Add("Token", options.ApiToken);
            }
            if (options.ShopId > 0)
            {
                client.DefaultRequestHeaders.Add("ShopId", options.ShopId.ToString());
            }
        });

        return services;
    }
}

