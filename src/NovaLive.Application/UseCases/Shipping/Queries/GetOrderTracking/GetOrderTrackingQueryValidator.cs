using FluentValidation;

namespace NovaLive.Application.UseCases.Shipping.Queries.GetOrderTracking;

public sealed class GetOrderTrackingQueryValidator : AbstractValidator<GetOrderTrackingQuery>
{
    public GetOrderTrackingQueryValidator() => RuleFor(x => x.OrderId).NotEmpty();
}
