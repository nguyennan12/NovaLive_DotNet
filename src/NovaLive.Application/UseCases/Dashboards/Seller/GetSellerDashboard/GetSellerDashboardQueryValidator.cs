using FluentValidation;

namespace NovaLive.Application.UseCases.Dashboards.Seller.GetSellerDashboard;

public sealed class GetSellerDashboardQueryValidator : AbstractValidator<GetSellerDashboardQuery>
{
    public GetSellerDashboardQueryValidator()
    {
        RuleFor(query => query).Must(query => query.From is null || query.To is null || query.From <= query.To)
            .WithMessage("from phải nhỏ hơn hoặc bằng to.");
    }
}
