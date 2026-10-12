using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class Sku : AuditableEntity, ISoftDeletable
{
    private Sku() { }

    public Sku(
        Guid spuId,
        Guid shopId,
        string skuCode,
        string? attributesJson,
        decimal originalPrice,
        decimal sellPrice,
        int weightGram = 200,
        bool isActive = true)
    {
        SpuId = spuId;
        ShopId = shopId;
        SkuCode = skuCode;
        AttributesJson = attributesJson;
        OriginalPrice = originalPrice;
        SellPrice = sellPrice;
        WeightGram = weightGram;
        IsActive = isActive;
    }

    public Guid SpuId { get; private set; }

    public Guid ShopId { get; private set; }

    public string SkuCode { get; private set; } = string.Empty;

    public string? AttributesJson { get; private set; }

    public decimal OriginalPrice { get; private set; }

    public decimal SellPrice { get; private set; }

    public int WeightGram { get; private set; } = 200;

    public bool IsActive { get; private set; } = true;

    public DateTimeOffset? DeletedAt { get; private set; }

    public void UpdatePriceAndDetails(
        decimal sellPrice,
        decimal originalPrice,
        int weightGram,
        bool isActive)
    {
        SellPrice = sellPrice;
        OriginalPrice = originalPrice;
        WeightGram = weightGram;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
