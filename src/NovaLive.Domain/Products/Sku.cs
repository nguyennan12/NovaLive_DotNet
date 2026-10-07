using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class Sku : AuditableEntity
{
    public Guid SpuId { get; set; }

    public Guid ShopId { get; set; }

    public string SkuCode { get; set; } = string.Empty;

    public string? AttributesJson { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal SellPrice { get; set; }

    public int WeightGram { get; set; } = 200;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? DeletedAt { get; set; }
}
