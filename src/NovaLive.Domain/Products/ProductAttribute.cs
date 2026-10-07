using NovaLive.Domain.Common;

namespace NovaLive.Domain.Products;

public sealed class ProductAttribute : Entity
{
    public Guid SpuId { get; private set; }

    public string AttrName { get; private set; } = string.Empty;

    public string AttrValue { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }
}
