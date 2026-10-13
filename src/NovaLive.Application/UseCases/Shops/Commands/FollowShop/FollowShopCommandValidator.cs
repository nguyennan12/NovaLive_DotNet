using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Commands.FollowShop;

public sealed class FollowShopCommandValidator : AbstractValidator<FollowShopCommand>
{
    public FollowShopCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
    }
}
