using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Commands.UnfollowShop;

public sealed class UnfollowShopCommandValidator : AbstractValidator<UnfollowShopCommand>
{
    public UnfollowShopCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
    }
}
