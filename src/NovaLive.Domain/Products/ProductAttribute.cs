using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class ProductAttribute : Entity
{
    public Guid SpuId { get; set; }

    public string AttrName { get; set; } = string.Empty;

    public string AttrValue { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
