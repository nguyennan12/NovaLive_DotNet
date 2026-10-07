using NovaLive.Domain.Common;

namespace NovaLive.Domain.Orders;

public sealed class ParentOrder : AuditableEntity
{
    public Guid BuyerId { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public string ShippingAddressJson { get; set; } = "{}";

    public decimal TotalItemAmount { get; set; }

    public decimal TotalShippingFee { get; set; }

    public decimal TotalDiscountAmount { get; set; }

    public decimal GrandTotal { get; set; }

    public string CurrencyCode { get; set; } = "VND";

    public ParentOrderPaymentStatus PaymentStatus { get; set; } = ParentOrderPaymentStatus.Pending;

    public string? Note { get; set; }
}
