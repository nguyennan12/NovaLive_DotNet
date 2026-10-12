using FluentValidation;

namespace NovaLive.Application.UseCases.Discounts.Commands.DeletePlatformDiscount;

public sealed class DeletePlatformDiscountCommandValidator : AbstractValidator<DeletePlatformDiscountCommand>
{
    public DeletePlatformDiscountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID voucher không được để trống.");
    }
}
