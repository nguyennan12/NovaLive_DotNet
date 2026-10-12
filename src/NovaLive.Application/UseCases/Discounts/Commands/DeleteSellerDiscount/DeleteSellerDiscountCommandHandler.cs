using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Commands.DeleteSellerDiscount;

public sealed class DeleteSellerDiscountCommandHandler(
    IDiscountRepository discountRepository,
    ICurrentUser currentUser)
    : ICommandHandler<DeleteSellerDiscountCommand>
{
    public async Task<Result> Handle(DeleteSellerDiscountCommand command, CancellationToken ct)
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

        if (discount.UsedCount > 0)
        {
            return DiscountsErrors.CannotDeleteUsedVoucher;
        }

        discountRepository.Delete(discount);
        await discountRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
