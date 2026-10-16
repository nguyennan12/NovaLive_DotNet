using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Livestreams;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.PinProduct;

public sealed class PinProductCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    ILivestreamProductRepository productRepository,
    ISkuRepository skuRepository,
    ISpuRepository spuRepository,
    IInventoryRepository inventoryRepository,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    IDateTimeProvider dateTimeProvider,
    ILivestreamNotificationService notificationService,
    IAfterCommitActions afterCommit)
    : ICommandHandler<PinProductCommand, PinnedProductResponse>
{
    public async Task<Result<PinnedProductResponse>> Handle(
        PinProductCommand command,
        CancellationToken ct)
    {
        var shopId = currentShop.ShopId ?? currentUser.ShopId;
        if (!shopId.HasValue || shopId.Value == Guid.Empty)
        {
            return LivestreamErrors.UnauthorizedShop;
        }

        var session = await sessionRepository.GetByIdAsync(command.SessionId, ct);
        if (session is null)
        {
            return LivestreamErrors.NotFound;
        }

        if (session.ShopId != shopId.Value)
        {
            return LivestreamErrors.ForbiddenShop;
        }

        if (session.Status != LivestreamStatus.Live)
        {
            return LivestreamErrors.NotLive;
        }

        var sku = await skuRepository.GetByIdAndShopAsync(command.Request.SkuId, shopId.Value, ct);
        if (sku is null)
        {
            return LivestreamErrors.SkuNotFound;
        }

        if (!sku.IsActive)
        {
            return LivestreamErrors.SkuInactive;
        }

        if (command.Request.FlashPrice.HasValue)
        {
            var flashPrice = command.Request.FlashPrice.Value;
            if (flashPrice <= 0 || flashPrice > sku.SellPrice)
            {
                return LivestreamErrors.InvalidFlashPrice;
            }
        }

        var inventory = await inventoryRepository.GetBySkuIdAndShopAsync(sku.Id, shopId.Value, ct);
        var availableQty = inventory?.AvailableQty ?? 0;

        if (command.Request.QuantityLimit.HasValue && command.Request.QuantityLimit.Value > availableQty)
        {
            return LivestreamErrors.QuantityLimitExceedsStock;
        }

        var spu = await spuRepository.GetByIdAsync(sku.SpuId, ct);
        if (spu is null)
        {
            return LivestreamErrors.SkuNotFound;
        }

        var sessionProducts = await productRepository.GetBySessionIdAsync(session.Id, ct);
        var currentPinned = sessionProducts.Where(p => p.IsPinned && p.SkuId != sku.Id).ToList();
        if (currentPinned.Count > 0)
        {
            foreach (var p in currentPinned)
            {
                p.Unpin();
            }
            productRepository.UpdateRange(currentPinned);
        }

        var now = dateTimeProvider.UtcNow;
        var durationSeconds = command.Request.DurationSeconds ?? 120;

        var liveProduct = sessionProducts.FirstOrDefault(p => p.SkuId == sku.Id);
        if (liveProduct is null)
        {
            liveProduct = new LivestreamProduct(
                session.Id,
                sku.Id,
                command.Request.FlashPrice,
                command.Request.QuantityLimit,
                displayOrder: 0,
                isPinned: true,
                pinnedAt: now);

            await productRepository.AddAsync(liveProduct, ct);
        }
        else
        {
            liveProduct.Pin(command.Request.FlashPrice, command.Request.QuantityLimit, 0, now);
            productRepository.Update(liveProduct);
        }

        var response = liveProduct.ToPinnedResponse(sku, spu, availableQty, durationSeconds);
        afterCommit.Add(async token => await notificationService.BroadcastProductPinnedAsync(session.Id, response, token));

        return response;
    }
}
