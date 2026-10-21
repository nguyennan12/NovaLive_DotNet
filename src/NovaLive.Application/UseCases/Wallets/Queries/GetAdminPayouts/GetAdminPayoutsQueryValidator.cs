using FluentValidation;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Wallets.Queries.GetAdminPayouts;

public sealed class GetAdminPayoutsQueryValidator : AbstractValidator<GetAdminPayoutsQuery>
{
    public GetAdminPayoutsQueryValidator()
    {
        RuleFor(x => x.Request.Page)
            .GreaterThan(0);

        RuleFor(x => x.Request.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.Request.Status)
            .Must(s => string.IsNullOrEmpty(s) || Enum.TryParse<PayoutStatus>(s, true, out _))
            .WithMessage("Trạng thái payout không hợp lệ");
    }
}
