using FluentValidation;

namespace NovaLive.Application.UseCases.Shipping.Queries.GetShippingLabel;

public sealed class GetShippingLabelQueryValidator : AbstractValidator<GetShippingLabelQuery>
{
    public GetShippingLabelQueryValidator() => RuleFor(x => x.SubOrderId).NotEmpty();
}
