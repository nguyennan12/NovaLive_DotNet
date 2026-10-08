namespace NovaLive.Contracts.V1.Products;

public record AdjustInventoryRequest(
    Guid SkuId,
    int QtyChange,
    string ChangeType,
    string? Note);
