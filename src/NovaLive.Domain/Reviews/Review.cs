using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public sealed class Review : AuditableEntity
{
    public Guid OrderItemId { get; private set; }

    public Guid SkuId { get; private set; }

    public Guid ShopId { get; private set; }

    public Guid BuyerId { get; private set; }

    public short Rating { get; private set; }

    public string? Title { get; private set; }

    public string? Content { get; private set; }

    public string? SellerReply { get; private set; }

    public DateTimeOffset? SellerRepliedAt { get; private set; }

    public short EditCount { get; private set; }

    public bool IsVisible { get; private set; } = true;
}
