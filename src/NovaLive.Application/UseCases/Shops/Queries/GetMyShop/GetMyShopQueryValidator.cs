using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Queries.GetMyShop;

public sealed class GetMyShopQueryValidator : AbstractValidator<GetMyShopQuery>
{
    // No client input; authentication and ownership are enforced by ShopAccess.
}
