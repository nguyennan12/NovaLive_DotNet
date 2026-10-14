using FluentValidation;

namespace NovaLive.Application.UseCases.FlashSales.Commands.RejectFlashSaleItem;

public sealed class RejectFlashSaleItemCommandValidator : AbstractValidator<RejectFlashSaleItemCommand>
{
    public RejectFlashSaleItemCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Mã sản phẩm Flash Sale không hợp lệ.");

        RuleFor(x => x.Request.Reason)
            .NotEmpty().WithMessage("Lý do từ chối không được để trống.")
            .MaximumLength(500).WithMessage("Lý do từ chối không được vượt quá 500 ký tự.");
    }
}
