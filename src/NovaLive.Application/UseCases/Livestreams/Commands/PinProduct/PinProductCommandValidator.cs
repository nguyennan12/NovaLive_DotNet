using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Commands.PinProduct;

public sealed class PinProductCommandValidator : AbstractValidator<PinProductCommand>
{
    public PinProductCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Mã phiên livestream không được để trống.");

        RuleFor(x => x.Request.SkuId)
            .NotEmpty().WithMessage("Mã SKU sản phẩm không được để trống.");

        When(x => x.Request.FlashPrice.HasValue, () =>
        {
            RuleFor(x => x.Request.FlashPrice!.Value)
                .GreaterThan(0).WithMessage("Giá Flash phải lớn hơn 0.");
        });

        When(x => x.Request.QuantityLimit.HasValue, () =>
        {
            RuleFor(x => x.Request.QuantityLimit!.Value)
                .GreaterThan(0).WithMessage("Giới hạn số lượng phải lớn hơn 0.");
        });

        When(x => x.Request.DurationSeconds.HasValue, () =>
        {
            RuleFor(x => x.Request.DurationSeconds!.Value)
                .InclusiveBetween(60, 300)
                .WithMessage("Thời hạn đếm ngược ghim sản phẩm phải từ 60 đến 300 giây.");
        });
    }
}
