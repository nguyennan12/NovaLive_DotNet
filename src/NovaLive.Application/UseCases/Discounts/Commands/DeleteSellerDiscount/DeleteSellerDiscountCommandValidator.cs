using FluentValidation;

namespace NovaLive.Application.UseCases.Discounts.Commands.DeleteSellerDiscount;

public sealed class DeleteSellerDiscountCommandValidator : AbstractValidator<DeleteSellerDiscountCommand>
{
    public DeleteSellerDiscountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID voucher không được để trống.");
    }
}
