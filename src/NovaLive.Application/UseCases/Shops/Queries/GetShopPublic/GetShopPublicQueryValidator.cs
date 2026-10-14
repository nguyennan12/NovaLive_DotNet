using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Queries.GetShopPublic;

public sealed class GetShopPublicQueryValidator : AbstractValidator<GetShopPublicQuery>
{
    public GetShopPublicQueryValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
    }
}
