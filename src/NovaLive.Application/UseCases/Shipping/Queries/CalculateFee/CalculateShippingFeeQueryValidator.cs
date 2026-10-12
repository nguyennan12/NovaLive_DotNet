using FluentValidation;

namespace NovaLive.Application.UseCases.Shipping.Queries.CalculateFee;

public sealed class CalculateShippingFeeQueryValidator : AbstractValidator<CalculateShippingFeeQuery>
{
    public CalculateShippingFeeQueryValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Dữ liệu tính phí không được để trống.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.ShopId)
                .NotEmpty().WithMessage("Mã gian hàng (ShopId) không hợp lệ.");

            RuleFor(x => x.Request.WarehouseAddressId)
                .NotEmpty().WithMessage("Mã kho xuất hàng (WarehouseAddressId) không hợp lệ.");

            RuleFor(x => x.Request.ShippingAddressId)
                .NotEmpty().WithMessage("Mã địa chỉ giao hàng (ShippingAddressId) không hợp lệ.");

            When(x => x.Request.WeightGram.HasValue, () =>
            {
                RuleFor(x => x.Request.WeightGram!.Value)
                    .GreaterThan(0).WithMessage("Khối lượng hàng phải lớn hơn 0 gram.")
                    .LessThanOrEqualTo(50_000).WithMessage("Khối lượng hàng không được vượt quá 50,000 gram (50kg).");
            });

            When(x => x.Request.Items != null && x.Request.Items.Count > 0, () =>
            {
                RuleForEach(x => x.Request.Items).ChildRules(item =>
                {
                    item.RuleFor(i => i.Quantity)
                        .GreaterThan(0).WithMessage("Số lượng từng mặt hàng phải lớn hơn 0.");

                    item.RuleFor(i => i.WeightGram)
                        .GreaterThan(0).WithMessage("Khối lượng từng mặt hàng phải lớn hơn 0.");
                });
            });
        });
    }
}
