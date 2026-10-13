using FluentValidation;
namespace NovaLive.Application.UseCases.Shops.Commands.DeleteShopAddress;

public sealed class DeleteShopAddressCommandValidator : AbstractValidator<DeleteShopAddressCommand>
{
    public DeleteShopAddressCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
