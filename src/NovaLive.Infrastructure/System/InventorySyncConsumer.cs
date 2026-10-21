using MassTransit;
using MediatR;
using NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Infrastructure.System;

public sealed class InventorySyncConsumer(ISender sender) : IConsumer<PaymentSuccessEvent>
{
    public async Task Consume(ConsumeContext<PaymentSuccessEvent> context)
    {
        var result = await sender.Send(new ConfirmInventorySaleCommand(context.Message.PaymentId), context.CancellationToken);
        // Transport failure must reach MassTransit's retry/error pipeline after the transaction rolled back.
        if (result.IsFailure) throw new InvalidOperationException($"Inventory sync failed: {result.Error.Code}.");
    }
}
