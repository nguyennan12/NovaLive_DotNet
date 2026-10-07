using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class Sku : AuditableEntity
{
    public Guid SpuId { get; private set; }

    public Guid ShopId { get; private set; }

    public string SkuCode { get; private set; } = string.Empty;

    public string? AttributesJson { get; private set; }

    public decimal OriginalPrice { get; private set; }

    public decimal SellPrice { get; private set; }

    public int WeightGram { get; private set; } = 200;

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset? DeletedAt { get; private set; }
}
