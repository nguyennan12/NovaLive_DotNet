using FluentValidation;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders.Queries.GetOrderDetail;
public sealed class GetOrderDetailQueryValidator : AbstractValidator<GetOrderDetailQuery>
{
    public GetOrderDetailQueryValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
