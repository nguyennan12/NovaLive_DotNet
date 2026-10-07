using NovaLive.Domain.Common;

namespace NovaLive.Domain.FlashSales;

public sealed class FlashSaleItem : Entity
{
    public Guid CampaignId { get; private set; }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public decimal FlashPrice { get; private set; }

    public int Quantity { get; private set; }

    public int PerUserLimit { get; private set; } = 1;

    public int ReservedQty { get; private set; }

    public int SoldQty { get; private set; }

    public FlashSaleItemStatus Status { get; private set; } = FlashSaleItemStatus.Pending;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
