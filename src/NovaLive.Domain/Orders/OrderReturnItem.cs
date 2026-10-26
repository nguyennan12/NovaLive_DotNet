using NovaLive.Domain.Common;
namespace NovaLive.Domain.Orders;
public sealed class OrderReturnItem : Entity
{
    private OrderReturnItem() { }
    public static OrderReturnItem Create(Guid returnId, Guid itemId, int quantity, string? detail) => new()
    { ReturnId = returnId, OrderItemId = itemId, Quantity = quantity, ReasonDetail = detail };
    public Guid ReturnId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public int Quantity { get; private set; }
    public string? ReasonDetail { get; private set; }
}
