using NovaLive.Domain.Common;

namespace NovaLive.Domain.FlashSales;

public sealed class FlashSaleItem : Entity
{
    public Guid CampaignId { get; set; }

    public Guid SkuId { get; set; }

    public Guid ShopId { get; set; }

    public decimal FlashPrice { get; set; }

    public int Quantity { get; set; }

    public int PerUserLimit { get; set; } = 1;

    public int ReservedQty { get; set; }

    public int SoldQty { get; set; }

    public FlashSaleItemStatus Status { get; set; } = FlashSaleItemStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
