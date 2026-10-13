using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Queries.GetShopAddresses;

public sealed class GetShopAddressesQueryValidator : AbstractValidator<GetShopAddressesQuery>
{
    // No client input; authentication and ownership are enforced by ShopAccess.
}
