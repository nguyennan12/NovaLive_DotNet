using NovaLive.Domain.Common;

namespace NovaLive.Domain.FlashSales;

public sealed class FlashSaleItem : Entity
{
    private FlashSaleItem() { }

    public FlashSaleItem(
        Guid campaignId,
        Guid skuId,
        Guid shopId,
        decimal flashPrice,
        int quantity,
        int perUserLimit)
    {
        if (campaignId == Guid.Empty)
        {
            throw new ArgumentException("Mã chiến dịch Flash Sale không hợp lệ.", nameof(campaignId));
        }

        if (skuId == Guid.Empty)
        {
            throw new ArgumentException("Mã SKU không hợp lệ.", nameof(skuId));
        }

        if (shopId == Guid.Empty)
        {
            throw new ArgumentException("Mã gian hàng không hợp lệ.", nameof(shopId));
        }

        if (flashPrice <= 0)
        {
            throw new ArgumentException("Giá Flash Sale phải lớn hơn 0.", nameof(flashPrice));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Số lượng sản phẩm Flash Sale phải lớn hơn 0.", nameof(quantity));
        }

        if (perUserLimit <= 0)
        {
            throw new ArgumentException("Giới hạn mua mỗi người dùng phải lớn hơn 0.", nameof(perUserLimit));
        }

        CampaignId = campaignId;
        SkuId = skuId;
        ShopId = shopId;
        FlashPrice = flashPrice;
        Quantity = quantity;
        PerUserLimit = perUserLimit;
        ReservedQty = 0;
        SoldQty = 0;
        Status = FlashSaleItemStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid CampaignId { get; private set; }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public decimal FlashPrice { get; private set; }

    public int Quantity { get; private set; }

    public int PerUserLimit { get; private set; } = 1;

    public int ReservedQty { get; private set; }

    public int SoldQty { get; private set; }

    public FlashSaleItemStatus Status { get; private set; } = FlashSaleItemStatus.Pending;

    public string? RejectionReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public void Approve()
    {
        if (Status != FlashSaleItemStatus.Pending)
        {
            throw new InvalidOperationException($"Chỉ có thể duyệt sản phẩm đang ở trạng thái Pending. Trạng thái hiện tại: {Status}.");
        }

        Status = FlashSaleItemStatus.Approved;
        RejectionReason = null;
    }

    public void Reject(string reason)
    {
        if (Status != FlashSaleItemStatus.Pending)
        {
            throw new InvalidOperationException($"Chỉ có thể từ chối sản phẩm đang ở trạng thái Pending. Trạng thái hiện tại: {Status}.");
        }

        Status = FlashSaleItemStatus.Rejected;
        RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Bị từ chối bởi quản trị viên." : reason.Trim();
    }
}
