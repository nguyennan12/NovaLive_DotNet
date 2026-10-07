using NovaLive.Domain.Common;

namespace NovaLive.Domain.Inventory;

public sealed class Inventory : Entity
{
    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public int QtyOnHand { get; private set; }

    public int ReservedQty { get; private set; }

    public int MinStock { get; private set; } = 5;

    public int AvailableQty => QtyOnHand - ReservedQty;

    public DateTimeOffset LastUpdated { get; private set; } = DateTimeOffset.UtcNow;
}
