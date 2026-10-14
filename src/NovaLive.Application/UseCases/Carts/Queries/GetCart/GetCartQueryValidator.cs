using FluentValidation;
namespace NovaLive.Application.UseCases.Carts.Queries.GetCart;
public sealed class GetCartQueryValidator : AbstractValidator<GetCartQuery>
{
    public GetCartQueryValidator()
    {
        // No client input: identity is supplied by the authenticated context.
    }
}
