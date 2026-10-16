using FluentValidation;

namespace NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;

public sealed class CalculateCheckoutDraftQueryValidator : AbstractValidator<CalculateCheckoutDraftQuery>
{
    public const int MaxCartItems = 100;

    public CalculateCheckoutDraftQueryValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.CartItemIds).Cascade(CascadeMode.Stop)
                .NotEmpty().Must(ids => ids.Count <= MaxCartItems)
                .WithMessage($"Chỉ được chọn tối đa {MaxCartItems} sản phẩm.")
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Danh sách sản phẩm không được trùng mã.");
            RuleForEach(x => x.Request.CartItemIds).NotEmpty();
            RuleFor(x => x.Request.ShippingAddressId).NotEmpty();
            RuleFor(x => x.Request.PlatformProductVoucherCode).NotEmpty().MaximumLength(50)
                .When(x => x.Request.PlatformProductVoucherCode is not null);
            RuleFor(x => x.Request.PlatformFreeshipVoucherCode).NotEmpty().MaximumLength(50)
                .When(x => x.Request.PlatformFreeshipVoucherCode is not null);
            RuleFor(x => x.Request.ShopVouchers).Must(v => v is null || v.Count <= MaxCartItems);
            RuleFor(x => x.Request.ShopVouchers)
                .Must(v => v is null || v.Where(s => s is not null).Select(s => s.ShopId).Distinct().Count() == v.Count)
                .WithMessage("Mỗi gian hàng chỉ được chọn một voucher.");
            RuleForEach(x => x.Request.ShopVouchers).NotNull().ChildRules(v =>
            {
                v.RuleFor(s => s.ShopId).NotEmpty();
                v.RuleFor(s => s.Code).NotEmpty().MaximumLength(50);
            });
        });
    }
}
