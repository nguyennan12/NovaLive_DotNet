using FluentValidation;

namespace NovaLive.Application.UseCases.FlashSales.Commands.ApproveFlashSaleItem;

public sealed class ApproveFlashSaleItemCommandValidator : AbstractValidator<ApproveFlashSaleItemCommand>
{
    public ApproveFlashSaleItemCommandValidator()
    {
        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Mã sản phẩm Flash Sale không hợp lệ.");
    }
}
