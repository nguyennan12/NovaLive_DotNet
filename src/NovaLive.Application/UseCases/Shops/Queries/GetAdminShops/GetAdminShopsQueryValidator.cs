using FluentValidation;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Queries.GetAdminShops;

public sealed class GetAdminShopsQueryValidator : AbstractValidator<GetAdminShopsQuery>
{
    public GetAdminShopsQueryValidator()
    {
        RuleFor(x => x.Request.Page)
            .GreaterThan(0);

        RuleFor(x => x.Request.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.Request.Status)
            .Must(s => string.IsNullOrEmpty(s) || Enum.TryParse<ShopStatus>(s, true, out _))
            .WithMessage("Trạng thái shop không hợp lệ");
    }
}
