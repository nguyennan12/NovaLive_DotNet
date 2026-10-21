using FluentValidation;

namespace NovaLive.Application.UseCases.Shops.Commands.ApproveShop;

public sealed class ApproveShopCommandValidator : AbstractValidator<ApproveShopCommand>
{
    public ApproveShopCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
        RuleFor(x => x.Request.Note)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Request.Note));
    }
}
