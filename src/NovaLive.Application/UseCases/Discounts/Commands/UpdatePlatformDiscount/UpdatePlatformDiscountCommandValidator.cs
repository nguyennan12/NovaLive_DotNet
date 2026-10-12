using FluentValidation;

namespace NovaLive.Application.UseCases.Discounts.Commands.UpdatePlatformDiscount;

public sealed class UpdatePlatformDiscountCommandValidator : AbstractValidator<UpdatePlatformDiscountCommand>
{
    public UpdatePlatformDiscountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID voucher không được để trống.");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên chương trình voucher không được để trống.")
            .MaximumLength(200).WithMessage("Tên chương trình tối đa 200 ký tự.");

        When(x => x.Request.MaxUses.HasValue, () =>
        {
            RuleFor(x => x.Request.MaxUses!.Value)
                .GreaterThan(0).WithMessage("Tổng số lượt dùng phải lớn hơn 0.");
        });
    }
}
