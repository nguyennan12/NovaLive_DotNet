using FluentValidation;
namespace NovaLive.Application.UseCases.Carts.Commands.UpdateCartItem;
public sealed class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemCommandValidator()
    {
        RuleFor(x => x.CartItemId).NotEmpty();
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () => RuleFor(x => x.Request.Quantity).GreaterThanOrEqualTo(0));
    }
}
