using FluentValidation;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Discounts.Commands.CreateSellerDiscount;

public sealed class CreateSellerDiscountCommandValidator : AbstractValidator<CreateSellerDiscountCommand>
{
    private static readonly string[] AllowedTypes = [DiscountType.PercentCart.ToString(), DiscountType.FixedCart.ToString()];
    private static readonly string[] AllowedAppliesTo = [DiscountAppliesTo.AllProducts.ToString(), DiscountAppliesTo.SpecificSpus.ToString(), DiscountAppliesTo.SpecificCategories.ToString()];

    public CreateSellerDiscountCommandValidator()
    {
        RuleFor(x => x.Request.Code)
            .NotEmpty().WithMessage("Mã voucher không được để trống.")
            .MaximumLength(50).WithMessage("Mã voucher tối đa 50 ký tự.")
            .Matches(@"^[a-zA-Z0-9_\-]+$").WithMessage("Mã voucher chỉ được chứa chữ cái, số, dấu gạch ngang hoặc gạch dưới.");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên chương trình voucher không được để trống.")
            .MaximumLength(200).WithMessage("Tên chương trình tối đa 200 ký tự.");

        RuleFor(x => x.Request.DiscountType)
            .NotEmpty().WithMessage("Loại giảm giá không được để trống.")
            .Must(type => AllowedTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Voucher gian hàng chỉ được chọn PercentCart hoặc FixedCart.");

        RuleFor(x => x.Request.DiscountValue)
            .GreaterThan(0).WithMessage("Giá trị giảm giá phải lớn hơn 0.");

        When(x => string.Equals(x.Request.DiscountType, DiscountType.PercentCart.ToString(), StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Request.DiscountValue)
                .LessThanOrEqualTo(100).WithMessage("Phần trăm giảm giá không được vượt quá 100%.");

            RuleFor(x => x.Request.MaxDiscountAmount)
                .NotNull().WithMessage("Giảm giá theo % bắt buộc phải có mức giảm tối đa.")
                .GreaterThan(0).WithMessage("Mức giảm tối đa phải lớn hơn 0.");
        });

        RuleFor(x => x.Request.MinOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Giá trị đơn hàng tối thiểu không được âm.");

        RuleFor(x => x.Request.PerUserLimit)
            .GreaterThan(0).WithMessage("Giới hạn mỗi khách hàng phải lớn hơn 0.");

        When(x => x.Request.MaxUses.HasValue, () =>
        {
            RuleFor(x => x.Request.MaxUses!.Value)
                .GreaterThan(0).WithMessage("Tổng số lượt dùng phải lớn hơn 0.");
        });

        RuleFor(x => x.Request.AppliesTo)
            .Must(applies => string.IsNullOrWhiteSpace(applies) || AllowedAppliesTo.Contains(applies, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Phạm vi áp dụng không hợp lệ.");

        When(x => string.Equals(x.Request.AppliesTo, DiscountAppliesTo.SpecificSpus.ToString(), StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.Request.TargetIds)
                .NotEmpty().WithMessage("Vui lòng chọn danh sách sản phẩm áp dụng.");
        });

        RuleFor(x => x.Request.ValidTo)
            .GreaterThan(x => x.Request.ValidFrom).WithMessage("Thời gian kết thúc (valid_to) phải lớn hơn thời gian bắt đầu (valid_from).");
    }
}
