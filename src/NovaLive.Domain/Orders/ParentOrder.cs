using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class ParentOrder : AuditableEntity
{
    private ParentOrder() { }
    public static ParentOrder Create(Guid buyerId, string code, string addressJson,
        decimal items, decimal shipping, decimal discounts, decimal total, string? note, DateTimeOffset now) => new()
    {
        BuyerId = buyerId, OrderCode = code, ShippingAddressJson = addressJson,
        TotalItemAmount = items, TotalShippingFee = shipping, TotalDiscountAmount = discounts,
        GrandTotal = total, Note = note, CreatedAt = now, UpdatedAt = now
    };

    public Guid BuyerId { get; private set; }

    public string OrderCode { get; private set; } = string.Empty;

    public string ShippingAddressJson { get; private set; } = "{}";

    public decimal TotalItemAmount { get; private set; }

    public decimal TotalShippingFee { get; private set; }

    public decimal TotalDiscountAmount { get; private set; }

    public decimal GrandTotal { get; private set; }

    public string CurrencyCode { get; private set; } = "VND";

    public ParentOrderPaymentStatus PaymentStatus { get; private set; } = ParentOrderPaymentStatus.Pending;

    public string? Note { get; private set; }
    public Result CancelPayment(DateTimeOffset now)
    {
        if (PaymentStatus == ParentOrderPaymentStatus.Cancelled) return Result.Failure(OrderStateErrors.AlreadyCancelled);
        if (PaymentStatus != ParentOrderPaymentStatus.Pending) return Result.Failure(OrderStateErrors.CannotCancelPaidOrderYet);
        PaymentStatus = ParentOrderPaymentStatus.Cancelled; UpdatedAt = now;
        return Result.Success();
    }
}
