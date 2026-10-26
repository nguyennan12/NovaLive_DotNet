namespace NovaLive.Application.Abstractions.Services;

public sealed record OrderPlacedSellerNotification(Guid SubOrderId, Guid ShopId, int ItemCount, decimal TotalAmount);

public interface IOrderNotificationService
{
    Task NotifySellerAsync(OrderPlacedSellerNotification notification, CancellationToken ct = default);
}
