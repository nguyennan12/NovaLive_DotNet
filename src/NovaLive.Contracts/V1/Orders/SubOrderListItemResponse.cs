namespace NovaLive.Contracts.V1.Orders;
public sealed record SubOrderListItemResponse(Guid Id, Guid ParentOrderId, string Code, Guid ShopId, decimal SubTotal, string Status, DateTimeOffset CreatedAt);
