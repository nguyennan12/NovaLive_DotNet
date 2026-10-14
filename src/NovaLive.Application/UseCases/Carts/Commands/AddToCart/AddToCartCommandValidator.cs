using FluentValidation;
namespace NovaLive.Application.UseCases.Carts.Commands.AddToCart;
public sealed class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.SkuId).NotEmpty();
            RuleFor(x => x.Request.Quantity).GreaterThan(0);
        });
    }
}
