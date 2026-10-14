using FluentValidation;

namespace NovaLive.Application.UseCases.FlashSales.Commands.RegisterSkuForFlashSale;

public sealed class RegisterSkuForFlashSaleCommandValidator : AbstractValidator<RegisterSkuForFlashSaleCommand>
{
    public RegisterSkuForFlashSaleCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Mã chiến dịch Flash Sale không hợp lệ.");

        RuleFor(x => x.Request.SkuId)
            .NotEmpty().WithMessage("Mã SKU không hợp lệ.");

        RuleFor(x => x.Request.FlashPrice)
            .GreaterThan(0).WithMessage("Giá Flash Sale phải lớn hơn 0.");

        RuleFor(x => x.Request.Quantity)
            .GreaterThan(0).WithMessage("Số lượng đăng ký phải lớn hơn 0.");

        RuleFor(x => x.Request.PerUserLimit)
            .GreaterThan(0).WithMessage("Giới hạn mua mỗi người dùng phải lớn hơn 0.");
    }
}
