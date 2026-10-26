using NovaLive.Domain.Common;

namespace NovaLive.Domain.Inventory;

public sealed class Inventory : Entity
{
    private Inventory() { }

    public Inventory(Guid skuId, Guid shopId, int initialStock, int minStock = 5)
    {
        if (initialStock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialStock), "Initial stock cannot be negative.");
        }

        if (minStock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minStock), "Min stock cannot be negative.");
        }

        SkuId = skuId;
        ShopId = shopId;
        QtyOnHand = initialStock;
        ReservedQty = 0;
        MinStock = minStock;
        Version = 1;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public int QtyOnHand { get; private set; }

    public int ReservedQty { get; private set; }

    public int MinStock { get; private set; } = 5;

    public int Version { get; private set; } = 1;

    public int AvailableQty => QtyOnHand - ReservedQty;

    public DateTimeOffset LastUpdated { get; private set; } = DateTimeOffset.UtcNow;

    public void AdjustOnHand(int qtyChange)
    {
        if (QtyOnHand + qtyChange < ReservedQty)
        {
            throw new InvalidOperationException("Số lượng tồn kho vật lý không thể nhỏ hơn số lượng đang giữ chỗ đặt hàng.");
        }

        if (QtyOnHand + qtyChange < 0)
        {
            throw new InvalidOperationException("Số lượng tồn kho vật lý không thể âm.");
        }

        QtyOnHand += qtyChange;
        Version++;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void Reserve(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Số lượng giữ chỗ phải lớn hơn 0.");
        }

        if (AvailableQty < qty)
        {
            throw new InvalidOperationException("Số lượng tồn kho khả dụng không đủ để giữ chỗ.");
        }

        ReservedQty += qty;
        Version++;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void ReleaseReservation(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Số lượng giải phóng phải lớn hơn 0.");
        }

        if (ReservedQty < qty)
        {
            throw new InvalidOperationException("Số lượng giải phóng vượt quá số lượng đang giữ chỗ.");
        }

        ReservedQty -= qty;
        Version++;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void ConfirmSale(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Số lượng xác nhận bán phải lớn hơn 0.");
        }

        if (ReservedQty < qty)
        {
            throw new InvalidOperationException("Số lượng xác nhận bán vượt quá số lượng đang giữ chỗ.");
        }

        if (QtyOnHand < qty)
        {
            throw new InvalidOperationException("Số lượng tồn kho vật lý không đủ để xuất kho.");
        }

        QtyOnHand -= qty;
        ReservedQty -= qty;
        Version++;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public Result ReceiveReturn(int qty, DateTimeOffset now)
    {
        if (qty <= 0 || (long)QtyOnHand + qty > int.MaxValue || Version == int.MaxValue)
            return InventoryReservationErrors.InvalidReturn;
        QtyOnHand += qty; Version++; LastUpdated = now;
        return Result.Success();
    }

    public void Return(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Số lượng trả hàng phải lớn hơn 0.");
        }

        QtyOnHand += qty;
        Version++;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void CancelSale(int qty)
    {
        if (qty <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(qty), "Số lượng hủy bán phải lớn hơn 0.");
        }

        QtyOnHand += qty;
        Version++;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void AdjustReserved(int reservedChange)
    {
        if (ReservedQty + reservedChange < 0)
        {
            throw new InvalidOperationException("Số lượng giữ chỗ không thể âm.");
        }

        if (QtyOnHand < ReservedQty + reservedChange)
        {
            throw new InvalidOperationException("Số lượng giữ chỗ không thể vượt quá tồn kho vật lý.");
        }

        ReservedQty += reservedChange;
        Version++;
        LastUpdated = DateTimeOffset.UtcNow;
    }
    public Result TryReserve(int quantity)
    {
        if (quantity <= 0 || AvailableQty < quantity)
            return Result.Failure(InventoryReservationErrors.InsufficientStock(SkuId));
        Reserve(quantity);
        return Result.Success();
    }
    public Result TryRelease(int quantity)
    {
        if (quantity <= 0 || ReservedQty < quantity)
            return Result.Failure(InventoryReservationErrors.InvalidRelease(SkuId));
        ReleaseReservation(quantity);
        return Result.Success();
    }

    public Result TryRelease(int quantity, DateTimeOffset now) => ChangeReservation(quantity, false, now);

    public Result TryConfirmSale(int quantity, DateTimeOffset now) => ChangeReservation(quantity, true, now);

    private Result ChangeReservation(int quantity, bool confirmSale, DateTimeOffset now)
    {
        if (quantity <= 0 || ReservedQty < quantity || QtyOnHand < quantity || Version == int.MaxValue)
            return InventoryReservationErrors.ReservationMismatch;

        ReservedQty -= quantity;
        if (confirmSale) QtyOnHand -= quantity;
        Version++;
        LastUpdated = now;
        return Result.Success();
    }
}
