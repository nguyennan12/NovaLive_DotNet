using MassTransit;

namespace NovaLive.Infrastructure.Search;

public sealed class ProductSearchVectorConsumerDefinition : ConsumerDefinition<ProductSearchVectorConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductSearchVectorConsumer> consumerConfigurator, IRegistrationContext context) =>
        endpointConfigurator.UseMessageRetry(retry => retry.Intervals(
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)));
}
