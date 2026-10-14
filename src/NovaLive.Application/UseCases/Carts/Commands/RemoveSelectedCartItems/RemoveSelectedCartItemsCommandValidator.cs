using FluentValidation;
namespace NovaLive.Application.UseCases.Carts.Commands.RemoveSelectedCartItems;
public sealed class RemoveSelectedCartItemsCommandValidator : AbstractValidator<RemoveSelectedCartItemsCommand>
{
    public RemoveSelectedCartItemsCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.CartItemIds).NotEmpty();
            RuleForEach(x => x.Request.CartItemIds).NotEmpty();
        });
    }
}
