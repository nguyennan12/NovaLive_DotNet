using FluentValidation;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Orders.Queries.GetMyOrders;
public sealed class GetMyOrdersQueryValidator : AbstractValidator<GetMyOrdersQuery>
{
    public GetMyOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).LessThanOrEqualTo(int.MaxValue / 100);
        RuleFor(x => x.Size).InclusiveBetween(1, 100);
        RuleFor(x => x).Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("from phải nhỏ hơn hoặc bằng to.");
        RuleFor(x => x.Status).Must(x => x is null || Enum.TryParse<ParentOrderPaymentStatus>(x, true, out var status) && Enum.IsDefined(status));
    }
}
