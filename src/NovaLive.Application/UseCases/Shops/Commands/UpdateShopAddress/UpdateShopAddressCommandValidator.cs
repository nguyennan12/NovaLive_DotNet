using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Commands.UpdateShopAddress;

public sealed class UpdateShopAddressCommandValidator : AbstractValidator<UpdateShopAddressCommand>
{
    public UpdateShopAddressCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request).SetValidator(new ShopAddressRequestValidator());
    }
}
