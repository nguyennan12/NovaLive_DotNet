using FluentValidation;

namespace NovaLive.Application.UseCases.FlashSales.Queries.GetFlashSaleProducts;

public sealed class GetFlashSaleProductsQueryValidator : AbstractValidator<GetFlashSaleProductsQuery>
{
    public GetFlashSaleProductsQueryValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Mã chiến dịch Flash Sale không hợp lệ.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100.");
    }
}
