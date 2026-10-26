using MassTransit;
using MediatR;
using NovaLive.Application.UseCases.Products.Commands.RebuildProductSearchVector;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Infrastructure.Search;

public sealed class ProductSearchVectorConsumer(ISender sender) : IConsumer<ProductUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var result = await sender.Send(new RebuildProductSearchVectorCommand(context.Message.SpuId), context.CancellationToken);
        // A Result failure must reach MassTransit's retry/error pipeline after transaction rollback.
        if (result.IsFailure) throw new InvalidOperationException($"Search vector rebuild failed: {result.Error.Code}.");
    }
}
