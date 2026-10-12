using FluentValidation;

namespace NovaLive.Application.UseCases.Discounts.Queries.ValidateVoucher;

public sealed class ValidateVoucherQueryValidator : AbstractValidator<ValidateVoucherQuery>
{
    public ValidateVoucherQueryValidator()
    {
        RuleFor(x => x.Request.Code)
            .NotEmpty().WithMessage("Mã voucher không được để trống.");

        RuleFor(x => x.Request.CartItemIds)
            .NotEmpty().WithMessage("Danh sách sản phẩm giỏ hàng không được để trống.");
    }
}
