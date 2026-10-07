using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class ParentOrder : AuditableEntity
{
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
}
