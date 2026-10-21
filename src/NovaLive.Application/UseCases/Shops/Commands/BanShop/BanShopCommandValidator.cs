using FluentValidation;

namespace NovaLive.Application.UseCases.Shops.Commands.BanShop;

public sealed class BanShopCommandValidator : AbstractValidator<BanShopCommand>
{
    public BanShopCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
        RuleFor(x => x.Request.Reason)
            .NotEmpty().WithMessage("Lý do cấm shop không được để trống")
            .MaximumLength(500).WithMessage("Lý do cấm shop không được vượt quá 500 ký tự");
    }
}
