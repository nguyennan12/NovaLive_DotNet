using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;

namespace NovaLive.Application.UseCases.Discounts.Commands.CreatePlatformDiscount;

public sealed class CreatePlatformDiscountCommandHandler(
    IDiscountRepository discountRepository)
    : ICommandHandler<CreatePlatformDiscountCommand, DiscountResponse>
{
    public async Task<Result<DiscountResponse>> Handle(CreatePlatformDiscountCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var normalizedCode = req.Code.Trim().ToUpperInvariant();

        if (await discountRepository.ExistsCodeAsync(normalizedCode, ct))
        {
            return DiscountsErrors.CodeAlreadyExists;
        }

        if (!Enum.TryParse<DiscountType>(req.DiscountType, true, out var discountType))
        {
            return DiscountsErrors.InvalidType;
        }

        var appliesTo = DiscountAppliesTo.AllProducts;
        if (!string.IsNullOrWhiteSpace(req.AppliesTo) &&
            Enum.TryParse<DiscountAppliesTo>(req.AppliesTo, true, out var parsedAppliesTo))
        {
            appliesTo = parsedAppliesTo;
        }

        var discountResult = Discount.CreatePlatformDiscount(
            code: normalizedCode,
            name: req.Name,
            discountType: discountType,
            discountValue: req.DiscountValue,
            minOrderAmount: req.MinOrderAmount,
            maxDiscountAmount: req.MaxDiscountAmount,
            maxUses: req.MaxUses,
            perUserLimit: req.PerUserLimit,
            appliesTo: appliesTo,
            targetIds: req.TargetIds,
            validFrom: req.ValidFrom,
            validTo: req.ValidTo,
            isPublic: req.IsPublic);

        if (discountResult.IsFailure)
        {
            return discountResult.Error;
        }

        var discount = discountResult.Value!;
        await discountRepository.AddAsync(discount, ct);
        await discountRepository.SaveChangesAsync(ct);

        return discount.ToResponse();
    }
}
