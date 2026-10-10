using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Events;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Products.Commands.DeleteSpu;

public sealed class DeleteSpuCommandHandler(
    ISpuRepository spuRepository,
    ISkuRepository skuRepository,
    IAppDbContext dbContext,
    ICurrentUser currentUser)
    : ICommandHandler<DeleteSpuCommand>
{
    public async Task<Result> Handle(DeleteSpuCommand command, CancellationToken ct)
    {
        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return ProductErrors.UnauthorizedShop;
        }

        var spu = await spuRepository.GetByIdAndShopAsync(command.SpuId, shopId.Value, ct);

        if (spu is null)
        {
            return ProductErrors.NotFound;
        }

        // 1. Soft-delete SPU
        spu.SoftDelete();
        spuRepository.Update(spu);

        // 2. Cascade soft-delete all child SKUs
        var skus = await skuRepository.GetBySpuIdAsync(spu.Id, ct);

        foreach (var sku in skus)
        {
            sku.SoftDelete();
            skuRepository.Update(sku);
        }

        // 3. Ghi OutboxMessage với typed Integration Event
        var integrationEvent = new ProductUpdatedIntegrationEvent(
            SpuId: spu.Id,
            ShopId: shopId.Value,
            Action: "Deleted");

        var outboxMessage = new OutboxMessage("ProductUpdatedEvent", JsonSerializer.Serialize(integrationEvent));
        await dbContext.OutboxMessages.AddAsync(outboxMessage, ct);

        return Result.Success();
    }
}
