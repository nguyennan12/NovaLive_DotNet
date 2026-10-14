using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;

public sealed class AddShopAddressCommandValidator : AbstractValidator<AddShopAddressCommand>
{
    public AddShopAddressCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request).SetValidator(new ShopAddressRequestValidator());
    }
}
