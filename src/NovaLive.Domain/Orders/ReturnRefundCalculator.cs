using NovaLive.Domain.Common;
namespace NovaLive.Domain.Orders;
public static class ReturnRefundCalculator
{
    public static Result<decimal> Calculate(OrderItem item, int quantity, long previouslyReturned = 0)
    {
        if (quantity <= 0 || item.Quantity <= 0 || previouslyReturned < 0 || previouslyReturned > item.Quantity - (long)quantity)
            return ReturnStateErrors.InvalidQuantity;
        // OrderItem.LineTotal already contains FinalLineTotal, after all product discounts.
        var paid = item.LineTotal;
        if (paid < 0 || item.DiscountAmount < 0) return ReturnStateErrors.InvalidSnapshot;
        // Match pricing proration's whole-VND floor. Only quantities are persisted per return item,
        // so reconstruct cumulative refunds under this policy; legacy amounts need reconciliation.
        var refundedBefore = Math.Floor(paid * previouslyReturned / item.Quantity);
        var returnedQuantity = previouslyReturned + quantity;
        if (returnedQuantity == item.Quantity)
            return paid - refundedBefore;
        return Math.Floor(paid * returnedQuantity / item.Quantity) - refundedBefore;
    }
}
