using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class ProductAttribute : Entity
{
    private ProductAttribute() { }

    public ProductAttribute(Guid spuId, string attrName, string attrValue, int displayOrder = 0)
    {
        SpuId = spuId;
        AttrName = attrName;
        AttrValue = attrValue;
        DisplayOrder = displayOrder;
    }

    public Guid SpuId { get; private set; }

    public string AttrName { get; private set; } = string.Empty;

    public string AttrValue { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }
}
