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

    /// <summary>
    /// Điều chỉnh số lượng tồn kho vật lý (Seller kiểm kê hoặc nhập hàng)
    /// </summary>
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

    /// <summary>
    /// Giữ chỗ kho khi người mua tạo đơn hàng (Checkout / Flash Sale Reserve)
    /// </summary>
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

    /// <summary>
    /// Giải phóng giữ chỗ khi đơn hàng bị hủy hoặc timeout thanh toán
    /// </summary>
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

    /// <summary>
    /// Xác nhận bán và xuất kho khi thanh toán thành công hoặc bàn giao vận chuyển
    /// </summary>
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

    /// <summary>
    /// Khách hoàn trả hàng về kho sau khi nhận
    /// </summary>
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

    /// <summary>
    /// Hủy đơn hàng đã xác nhận bán trước khi xuất giao (hoàn lại tồn kho vật lý)
    /// </summary>
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
}
