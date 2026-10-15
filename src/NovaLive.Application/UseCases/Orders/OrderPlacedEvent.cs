using NovaLive.Contracts.V1.Orders;
namespace NovaLive.Application.UseCases.Orders;
public sealed record OrderPlacedEvent(Guid ParentOrderId, string OrderCode, Guid BuyerId,
    IReadOnlyList<OrderPlacedShop> SubOrders, string PaymentMethod, decimal GrandTotal,
    string IdempotencyKey, CheckoutResponse Receipt);
