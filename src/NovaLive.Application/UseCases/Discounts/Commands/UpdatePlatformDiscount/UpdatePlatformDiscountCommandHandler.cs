using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Commands.UpdatePlatformDiscount;

public sealed class UpdatePlatformDiscountCommandHandler(
    IDiscountRepository discountRepository)
    : ICommandHandler<UpdatePlatformDiscountCommand, DiscountResponse>
{
    public async Task<Result<DiscountResponse>> Handle(UpdatePlatformDiscountCommand command, CancellationToken ct)
    {
        var discount = await discountRepository.GetByIdAsync(command.Id, ct);
        if (discount is null)
        {
            return DiscountsErrors.NotFound;
        }

        if (discount.ShopId.HasValue)
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
