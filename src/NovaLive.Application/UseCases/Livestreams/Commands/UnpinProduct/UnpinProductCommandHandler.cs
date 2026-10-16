using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;
using NovaLive.Domain.Livestreams;

namespace NovaLive.Application.UseCases.Livestreams.Commands.UnpinProduct;

public sealed class UnpinProductCommandHandler(
    ILivestreamSessionRepository sessionRepository,
    ILivestreamProductRepository productRepository,
    ICurrentUser currentUser,
    ICurrentShop currentShop,
    ILivestreamNotificationService notificationService,
    IAfterCommitActions afterCommit)
    : ICommandHandler<UnpinProductCommand>
{
    public async Task<Result> Handle(UnpinProductCommand command, CancellationToken ct)
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

        LivestreamProduct? productToUnpin;
        if (command.SkuId.HasValue)
        {
            productToUnpin = await productRepository.GetBySessionAndSkuAsync(session.Id, command.SkuId.Value, ct);
            if (productToUnpin is null || !productToUnpin.IsPinned)
            {
                return LivestreamErrors.ProductNotPinned;
            }
        }
        else
        {
            productToUnpin = await productRepository.GetActivePinnedProductAsync(session.Id, ct);
            if (productToUnpin is null)
            {
                return Result.Success();
            }
        }

        productToUnpin.Unpin();
        productRepository.Update(productToUnpin);

        afterCommit.Add(async token => await notificationService.BroadcastProductUnpinnedAsync(session.Id, productToUnpin.SkuId, token));

        return Result.Success();
    }
}
