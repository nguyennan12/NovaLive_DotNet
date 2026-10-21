using MassTransit;

namespace NovaLive.Infrastructure.System;

public sealed class InventorySyncConsumerDefinition : ConsumerDefinition<InventorySyncConsumer>
{
    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<InventorySyncConsumer> consumerConfigurator, IRegistrationContext context) =>
        endpointConfigurator.UseMessageRetry(retry => retry.Intervals(
            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)));
}
