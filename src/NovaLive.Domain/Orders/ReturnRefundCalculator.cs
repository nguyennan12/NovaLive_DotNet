using NovaLive.Domain.Common;
namespace NovaLive.Domain.Orders;
public static class ReturnRefundCalculator
{
    public static Result<decimal> Calculate(OrderItem item, int quantity, long previouslyReturned = 0)
    {
        if (quantity <= 0 || item.Quantity <= 0 || previouslyReturned < 0 || previouslyReturned + quantity > item.Quantity)
            return ReturnStateErrors.InvalidQuantity;
        // Follow the agreed return formula against immutable order snapshots, excluding shipping.
        var paid = item.LineTotal - item.DiscountAmount;
        if (paid < 0 || item.DiscountAmount < 0) return ReturnStateErrors.InvalidSnapshot;
        // Cumulative rounding keeps multiple partial refunds within the original line amount.
        return decimal.Round(paid * (previouslyReturned + quantity) / item.Quantity, 2, MidpointRounding.AwayFromZero)
            - decimal.Round(paid * previouslyReturned / item.Quantity, 2, MidpointRounding.AwayFromZero);
    }
}
