namespace NovaLive.Contracts.V1.Orders;
public sealed record CheckoutSubOrderResponse(Guid Id, string Code, Guid ShopId, decimal SubTotal, string Status);
