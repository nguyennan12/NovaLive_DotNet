using FluentValidation;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders.Queries.GetSellerOrderDetail;
public sealed class GetSellerOrderDetailQueryValidator : AbstractValidator<GetSellerOrderDetailQuery>
{
    public GetSellerOrderDetailQueryValidator()
    {
        RuleFor(x => x.SubOrderId).NotEmpty();
    }
}
