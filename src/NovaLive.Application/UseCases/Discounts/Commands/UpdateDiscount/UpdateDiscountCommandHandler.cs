using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Commands.UpdateDiscount;

public sealed class UpdateDiscountCommandHandler(
    IDiscountRepository discountRepository,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateDiscountCommand, DiscountResponse>
{
    public async Task<Result<DiscountResponse>> Handle(UpdateDiscountCommand command, CancellationToken ct)
    {
        var discount = await discountRepository.GetByIdAsync(command.Id, ct);
        if (discount is null)
        {
            return DiscountsErrors.NotFound;
        }

        var shopId = currentUser.ShopId;
        if (!shopId.HasValue || discount.ShopId != shopId.Value)
        {
            return DiscountsErrors.ForbiddenShop;
        }

        var updateResult = discount.Update(
            name: command.Request.Name,
            maxUses: command.Request.MaxUses,
            validTo: command.Request.ValidTo,
            isActive: command.Request.IsActive);

        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }

        discountRepository.Update(discount);
        await discountRepository.SaveChangesAsync(ct);

        return discount.ToResponse();
    }
}
