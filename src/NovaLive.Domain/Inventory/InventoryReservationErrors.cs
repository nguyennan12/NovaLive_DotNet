using NovaLive.Domain.Common;
namespace NovaLive.Domain.Inventory;
public static class InventoryReservationErrors
{
    public static readonly Error ReservationMismatch = Error.Conflict("Inventory.ReservationMismatch", "Số lượng giữ chỗ không khớp với đơn hàng hoặc vượt giới hạn lưu trữ.");
    public static readonly Error InvalidReturn = Error.Validation("Returns.InvalidInventoryQuantity", "Số lượng hoàn kho không hợp lệ hoặc vượt giới hạn lưu trữ.");
    public static Error InsufficientStock(Guid sku) => Error.Validation("Inventory.InsufficientStock", $"SKU '{sku}' không đủ tồn kho.");
    public static Error InvalidRelease(Guid sku) => Error.Conflict("Inventory.InvalidRelease", $"SKU '{sku}' không đủ giữ chỗ để hoàn.");
}
