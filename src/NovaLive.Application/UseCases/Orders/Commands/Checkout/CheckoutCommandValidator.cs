using FluentValidation;
using NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;
using NovaLive.Contracts.V1.Orders;
namespace NovaLive.Application.UseCases.Orders.Commands.Checkout;
public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => new CalculateCheckoutDraftQuery(new CalculateCheckoutRequest(
                x.Request.CartItemIds.ToList(), x.Request.ShippingAddressId, x.Request.ShopVouchers == null ? null : x.Request.ShopVouchers.ToList(),
                x.Request.PlatformProductVoucherCode, x.Request.PlatformFreeshipVoucherCode)))
                .SetValidator(new CalculateCheckoutDraftQueryValidator())
                .When(x => x.Request.CartItemIds is not null);
            RuleFor(x => x.Request.CartItemIds).NotEmpty();
            RuleFor(x => x.Request.PaymentMethod).Must(x => x is "MoMo" or "VietQR" or "COD");
            RuleFor(x => x.Request.Note).MaximumLength(2000);
            RuleFor(x => x.Request.ShippingProviders).NotEmpty()
                .Must(x => x is null || x.Select(s => s?.ShopId).Distinct().Count() == x.Count);
            RuleForEach(x => x.Request.ShippingProviders).NotNull().ChildRules(v =>
            {
                v.RuleFor(s => s.ShopId).NotEmpty();
                v.RuleFor(s => s.Provider).NotEmpty().MaximumLength(20);
                v.RuleFor(s => s.ServiceCode).NotEmpty().MaximumLength(50);
            });
        });
    }
}
