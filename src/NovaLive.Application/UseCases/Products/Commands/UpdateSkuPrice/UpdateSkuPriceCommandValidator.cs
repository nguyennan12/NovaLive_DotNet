using FluentValidation;

namespace NovaLive.Application.UseCases.Products.Commands.UpdateSkuPrice;

public sealed class UpdateSkuPriceCommandValidator : AbstractValidator<UpdateSkuPriceCommand>
{
    public UpdateSkuPriceCommandValidator()
    {
        RuleFor(x => x.SkuId)
            .NotEmpty().WithMessage("ID biến thể SKU không được để trống.");

        RuleFor(x => x.Request)
            .NotNull().WithMessage("Dữ liệu cập nhật SKU không được để trống.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.SellPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Giá bán SKU phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.Request.OriginalPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Giá gốc SKU phải lớn hơn hoặc bằng 0.");

            RuleFor(x => x.Request.WeightGram)
                .GreaterThan(0).WithMessage("Trọng lượng SKU phải lớn hơn 0 gram.");
        });
    }
}
