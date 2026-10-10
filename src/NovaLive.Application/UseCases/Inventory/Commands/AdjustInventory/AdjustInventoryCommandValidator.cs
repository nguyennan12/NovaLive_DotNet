using FluentValidation;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Inventory.Commands.AdjustInventory;

public sealed class AdjustInventoryCommandValidator : AbstractValidator<AdjustInventoryCommand>
{
    private static readonly string[] AllowedChangeTypes =
    [
        InventoryChangeType.Import.ToString(),
        InventoryChangeType.ManualAdjust.ToString()
    ];

    public AdjustInventoryCommandValidator()
    {
        RuleFor(x => x.Request.SkuId)
            .NotEmpty()
            .WithMessage("Mã biến thể sản phẩm (SKU) không được để trống.");

        RuleFor(x => x.Request.QtyChange)
            .NotEqual(0)
            .WithMessage("Số lượng thay đổi tồn kho phải khác 0.");

        RuleFor(x => x.Request.ChangeType)
            .NotEmpty()
            .WithMessage("Loại điều chỉnh tồn kho không được để trống.")
            .Must(type => AllowedChangeTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Loại điều chỉnh không hợp lệ. Seller chỉ được thực hiện Import hoặc ManualAdjust.");

        RuleFor(x => x.Request.Note)
            .MaximumLength(500)
            .WithMessage("Ghi chú không được vượt quá 500 ký tự.");
    }
}
