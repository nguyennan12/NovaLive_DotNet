using NovaLive.Domain.Common;

namespace NovaLive.Domain.Inventory;

public sealed class Inventory : Entity
{
    public Guid SkuId { get; set; }

    public Guid ShopId { get; set; }

    public int QtyOnHand { get; set; }

    public int ReservedQty { get; set; }

    public int MinStock { get; set; } = 5;

    public int AvailableQty => QtyOnHand - ReservedQty;

    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
}
