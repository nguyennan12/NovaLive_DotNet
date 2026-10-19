using FluentValidation;

namespace NovaLive.Application.UseCases.Dashboards.Admin.GetAdminDashboard;

public sealed class GetAdminDashboardQueryValidator : AbstractValidator<GetAdminDashboardQuery>
{
    public GetAdminDashboardQueryValidator()
    {
        RuleFor(query => query).Must(query => query.From is null || query.To is null || query.From <= query.To)
            .WithMessage("from phải nhỏ hơn hoặc bằng to.");
    }
}
