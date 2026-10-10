using NovaLive.Domain.Common;

namespace NovaLive.Domain.Inventory;

public sealed class Inventory : Entity
{
    private Inventory() { }

    public Inventory(Guid skuId, Guid shopId, int initialStock, int minStock = 5)
    {
        SkuId = skuId;
        ShopId = shopId;
        QtyOnHand = initialStock;
        ReservedQty = 0;
        MinStock = minStock;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public int QtyOnHand { get; private set; }

    public int ReservedQty { get; private set; }

    public int MinStock { get; private set; } = 5;

    public int AvailableQty => QtyOnHand - ReservedQty;

    public DateTimeOffset LastUpdated { get; private set; } = DateTimeOffset.UtcNow;

    public void AdjustOnHand(int qtyChange)
    {
        QtyOnHand += qtyChange;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public void AdjustReserved(int reservedChange)
    {
        ReservedQty += reservedChange;
        LastUpdated = DateTimeOffset.UtcNow;
    }
}
