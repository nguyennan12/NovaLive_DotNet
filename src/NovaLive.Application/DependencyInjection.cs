using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.Common.Behaviors;

namespace NovaLive.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // MediatR Pipeline Behaviors registered in execution order:
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            configuration.AddOpenBehavior(typeof(IdempotencyBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<UseCases.Auth.Common.AuthSessionService>();
        services.AddScoped<UseCases.Auth.Common.OtpFlowService>();
        services.AddOptions<UseCases.Shops.KycUrlOptions>();
        services.AddScoped<UseCases.Shops.ShopAccess>();
        services.AddScoped<UseCases.Shops.ShopAddressDefaults>();

        return services;
    }
}
