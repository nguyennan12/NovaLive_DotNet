using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public sealed class Review : AuditableEntity
{
    public Guid OrderItemId { get; set; }

    public Guid SkuId { get; set; }

    public Guid ShopId { get; set; }

    public Guid BuyerId { get; set; }

    public short Rating { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? SellerReply { get; set; }

    public DateTimeOffset? SellerRepliedAt { get; set; }

    public short EditCount { get; set; }

    public bool IsVisible { get; set; } = true;
}
