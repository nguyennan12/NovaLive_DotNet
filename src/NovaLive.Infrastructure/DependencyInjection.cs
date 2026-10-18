using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
using Minio;
using NovaLive.Infrastructure.Storage;
using NovaLive.Infrastructure.Livestreams.Agora;
using NovaLive.Infrastructure.Payments.Momo;
using NovaLive.Infrastructure.Payments.VietQr;

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
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            options.AddInterceptors(
                serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
                serviceProvider.GetRequiredService<SoftDeleteInterceptor>(),
                serviceProvider.GetRequiredService<DomainEventsDispatcherInterceptor>());
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IAuthPersistence, AuthPersistence>();
        services.Configure<NovaLive.Application.UseCases.Shops.KycUrlOptions>(
            configuration.GetSection(NovaLive.Application.UseCases.Shops.KycUrlOptions.SectionName));
        services.AddScoped<IAfterCommitActions, AfterCommitActions>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IProductQueryService, PostgresProductQueryService>();

        // Repositories
        services.AddScoped<IOrderReturnRepository, OrderReturnRepository>();
        services.AddScoped<IShopWalletTransactionRepository, ShopWalletTransactionRepository>();
        services.AddScoped<IInventoryHistoryRepository, InventoryHistoryRepository>();
        services.AddScoped<IOrderReturnItemRepository, OrderReturnItemRepository>();
        services.AddScoped<IShopRepository, ShopRepository>();
        services.AddScoped<IUserRepository, NovaLive.Infrastructure.Persistence.Repositories.UserRepository>();
        services.AddScoped<IPermissionRepository, NovaLive.Infrastructure.Persistence.Repositories.PermissionRepository>();
        services.AddScoped<ISpuRepository, NovaLive.Infrastructure.Persistence.Repositories.SpuRepository>();
        services.AddScoped<ISkuRepository, NovaLive.Infrastructure.Persistence.Repositories.SkuRepository>();
        services.AddScoped<IInventoryRepository, NovaLive.Infrastructure.Persistence.Repositories.InventoryRepository>();
        services.AddScoped<IDiscountRepository, NovaLive.Infrastructure.Persistence.Repositories.DiscountRepository>();
        services.AddScoped<IParentOrderRepository, ParentOrderRepository>();
        services.AddScoped<ISubOrderRepository, SubOrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICartRepository, NovaLive.Infrastructure.Persistence.Repositories.CartRepository>();
        services.AddScoped<IUserAddressRepository, UserAddressRepository>();
        services.AddScoped<IShopAddressRepository, ShopAddressRepository>();
        services.AddScoped<IShippingOrderRepository, ShippingOrderRepository>();
        services.AddScoped<IShippingProviderResolver, NovaLive.Infrastructure.Shipping.ShippingProviderResolver>();
        services.AddScoped<ICategoryRepository, NovaLive.Infrastructure.Persistence.Repositories.CategoryRepository>();
        services.AddScoped<IFlashSaleRepository, NovaLive.Infrastructure.Persistence.Repositories.FlashSaleRepository>();
        services.AddScoped<IShopWalletRepository, ShopWalletRepository>();
        services.AddScoped<ILivestreamSessionRepository, LivestreamSessionRepository>();
        services.AddScoped<ILivestreamProductRepository, LivestreamProductRepository>();
        services.AddScoped<ILivestreamCommentRepository, LivestreamCommentRepository>();

        // Migration & Seeding
        services.AddScoped<IDataSeeder, RbacDataSeeder>();
        services.AddScoped<IDataSeeder, CategoryDataSeeder>();
        services.AddScoped<IDataSeeder, ProductDataSeeder>();
        services.AddScoped<IMigrationService, MigrationService>();

        // Caching & Idempotency
        var redisConnectionString = configuration.GetValue<string>("Redis:ConnectionString");
        if (string.IsNullOrWhiteSpace(redisConnectionString))
            throw new InvalidOperationException("Redis:ConnectionString is required for authentication and revocation checks.");

        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectRetry = 5;
        redisOptions.ConnectTimeout = 5000;
        redisOptions.SyncTimeout = 5000;
        redisOptions.KeepAlive = 60;

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IIdempotencyService, RedisIdempotencyService>();
        services.AddSingleton<ILivestreamCacheManager, LivestreamCacheManager>();

        // Outbox background worker
        services.AddHostedService<OutboxBackgroundWorker>();
        services.AddHostedService<NovaLive.Infrastructure.Shipping.ShippingStatusPollingWorker>();

        // Messaging
        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<OrderPlacedConsumer>();
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

        services.AddHttpClient<ResendOrderConfirmationEmailSender>(client => client.Timeout = TimeSpan.FromSeconds(10))
            .RemoveAllLoggers()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddTransient<LoggingOrderConfirmationEmailSender>();
        services.AddTransient<IOrderConfirmationEmailSender>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<EmailOptions>>().Value;
            return settings.IsLogging
                ? provider.GetRequiredService<LoggingOrderConfirmationEmailSender>()
                : provider.GetRequiredService<ResendOrderConfirmationEmailSender>();
        });

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

        // MinIO File Storage
        services.AddOptions<MinioOptions>()
            .Bind(configuration.GetSection(MinioOptions.SectionName))
            .PostConfigure(options =>
            {
                var envEndpoint = configuration["MINIO_ENDPOINT"];
                if (!string.IsNullOrWhiteSpace(envEndpoint))
                {
                    options.Endpoint = envEndpoint;
                }

                var rootUser = configuration["MINIO_ROOT_USER"];
                if (!string.IsNullOrWhiteSpace(rootUser) && string.IsNullOrWhiteSpace(options.AccessKey))
                {
                    options.AccessKey = rootUser;
                }

                var rootPass = configuration["MINIO_ROOT_PASSWORD"];
                if (!string.IsNullOrWhiteSpace(rootPass) && string.IsNullOrWhiteSpace(options.SecretKey))
                {
                    options.SecretKey = rootPass;
                }
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.Endpoint), "Minio:Endpoint must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.AccessKey), "Minio:AccessKey must be configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "Minio:SecretKey must be configured.")
            .ValidateOnStart();

        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            var client = new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey);

            if (options.UseSsl)
            {
                client = client.WithSSL();
            }

            return client.Build();
        });

        services.AddSingleton<MinioFileStorageService>();
        services.AddSingleton<IFileStorageService>(sp => sp.GetRequiredService<MinioFileStorageService>());
        services.AddHostedService<MinioBucketInitializer>();

        services.Configure<AgoraOptions>(options =>
        {
            configuration.GetSection(AgoraOptions.SectionName).Bind(options);

            var envAppId = configuration["AGORA_APP_ID"];
            if (!string.IsNullOrWhiteSpace(envAppId))
            {
                options.AppId = envAppId;
            }

            var envAppCert = configuration["AGORA_APP_CERTIFICATE"];
            if (!string.IsNullOrWhiteSpace(envAppCert))
            {
                options.AppCertificate = envAppCert;
            }

            var envTtlStr = configuration["AGORA_TOKEN_TTL_SECONDS"];
            if (int.TryParse(envTtlStr, out var envTtl) && envTtl > 0)
            {
                options.TokenTtlSeconds = envTtl;
            }
        });
        services.AddSingleton<IAgoraTokenService, AgoraTokenService>();
        // MoMo Payment Gateway
        services.Configure<MomoOptions>(options =>
        {
            var section = configuration.GetSection(MomoOptions.SectionName);
            section.Bind(options);

            var partnerCode = configuration["MOMO_PARTNER_CODE"];
            if (!string.IsNullOrWhiteSpace(partnerCode)) options.PartnerCode = partnerCode;

            var accessKey = configuration["MOMO_ACCESS_KEY"];
            if (!string.IsNullOrWhiteSpace(accessKey)) options.AccessKey = accessKey;

            var secretKey = configuration["MOMO_SECRET_KEY"];
            if (!string.IsNullOrWhiteSpace(secretKey)) options.SecretKey = secretKey;

            var endpoint = configuration["MOMO_ENDPOINT"];
            if (!string.IsNullOrWhiteSpace(endpoint)) options.Endpoint = endpoint;

            var redirectUrl = configuration["MOMO_REDIRECT_URL"];
            if (!string.IsNullOrWhiteSpace(redirectUrl)) options.RedirectUrl = redirectUrl;

            var ipnUrl = configuration["MOMO_IPN_URL"];
            if (!string.IsNullOrWhiteSpace(ipnUrl)) options.IpnUrl = ipnUrl;
        });

        services.AddHttpClient<IMomoPaymentAdapter, MomoPaymentAdapter>();
        services.AddScoped<IPaymentGatewayAdapter>(sp => sp.GetRequiredService<IMomoPaymentAdapter>());

        // VietQR Payment Gateway
        services.Configure<VietQrOptions>(options =>
        {
            var section = configuration.GetSection(VietQrOptions.SectionName);
            section.Bind(options);

            var bankBin = configuration["VIETQR_BANK_BIN"];
            if (!string.IsNullOrWhiteSpace(bankBin)) options.BankBin = bankBin;

            var accountNumber = configuration["VIETQR_ACCOUNT_NUMBER"];
            if (!string.IsNullOrWhiteSpace(accountNumber)) options.AccountNumber = accountNumber;

            var accountName = configuration["VIETQR_ACCOUNT_NAME"];
            if (!string.IsNullOrWhiteSpace(accountName)) options.AccountName = accountName;

            var clientId = configuration["VIETQR_CLIENT_ID"] ?? configuration["VIETQR_USERNAME"];
            if (!string.IsNullOrWhiteSpace(clientId)) options.ClientId = clientId.Trim();

            var apiKey = configuration["VIETQR_API_KEY"] ?? configuration["VIETQR_PASSWORD"];
            if (!string.IsNullOrWhiteSpace(apiKey)) options.ApiKey = apiKey.Trim();

            var webhookSecret = configuration["VIETQR_WEBHOOK_SECRET"];
            if (!string.IsNullOrWhiteSpace(webhookSecret)) options.WebhookSecret = webhookSecret.Trim();

            var baseUrl = configuration["VIETQR_BASE_URL"] ?? configuration["VIETQR_API_BASE_URL"];
            if (!string.IsNullOrWhiteSpace(baseUrl)) options.BaseUrl = baseUrl.Trim();
        });

        services.AddSingleton<IVietQrPaymentAdapter, VietQrPaymentAdapter>();
        services.AddSingleton<IPaymentGatewayAdapter>(sp => sp.GetRequiredService<IVietQrPaymentAdapter>());

        return services;
    }
}

