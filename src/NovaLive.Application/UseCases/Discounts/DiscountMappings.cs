using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Discounts;

namespace NovaLive.Application.UseCases.Discounts;

public static class DiscountMappings
{
    public static DiscountResponse ToResponse(this Discount discount) =>
        new(
            discount.Id,
            discount.ShopId,
            discount.Code,
            discount.Name,
            discount.DiscountType.ToString(),
            discount.DiscountValue,
            discount.MinOrderAmount,
            discount.MaxDiscountAmount,
            discount.MaxUses,
            discount.UsedCount,
            discount.PerUserLimit,
            discount.AppliesTo.ToString(),
            discount.TargetIds,
            discount.ValidFrom,
            discount.ValidTo,
            discount.IsPublic,
            discount.IsActive,
            discount.CreatedAt);
}
