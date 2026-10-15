namespace NovaLive.Application.UseCases.Orders;
public sealed record OrderPlacedShop(Guid SubOrderId, Guid ShopId, int ItemCount, decimal TotalAmount);
