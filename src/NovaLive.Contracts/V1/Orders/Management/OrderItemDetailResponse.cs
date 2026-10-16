namespace NovaLive.Contracts.V1.Orders;
public sealed record OrderItemDetailResponse(Guid Id, Guid SkuId, string SkuSnapshotJson, int Quantity, decimal UnitPrice, decimal OriginalPrice, decimal DiscountAmount, decimal LineTotal);
