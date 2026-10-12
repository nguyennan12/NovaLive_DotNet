using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Discounts;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Commands.DeletePlatformDiscount;

public sealed class DeletePlatformDiscountCommandHandler(
    IDiscountRepository discountRepository)
    : ICommandHandler<DeletePlatformDiscountCommand>
{
    public async Task<Result> Handle(DeletePlatformDiscountCommand command, CancellationToken ct)
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

        if (discount.UsedCount > 0)
        {
            return DiscountsErrors.CannotDeleteUsedVoucher;
        }

        discountRepository.Delete(discount);
        await discountRepository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
