using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping;

public static class ShippingStatusMapper
{
    public static ShippingStatus? Map(ShippingProvider provider, string? status)
    {
        // Normalized statuses are also accepted by future provider adapters.
        if (Enum.TryParse<ShippingStatus>(status?.Trim(), true, out var normalized) && Enum.IsDefined(normalized))
            return normalized;
        if (provider != ShippingProvider.GHN) return null;
        return status?.Trim().ToLowerInvariant() switch
        {
            "ready_to_pick" or "ready_for_pick" => ShippingStatus.ReadyToPick,
            "picking" or "money_collect_picking" or "picked" or "storing" => ShippingStatus.Picking,
            "sorting" or "transporting" or "money_collect_delivering" => ShippingStatus.Delivering,
            "delivery_fail" or "return_fail" or "exception" or "lost" or "damage" or "scrap" => ShippingStatus.Failed,
            "waiting_to_return" or "return" or "return_transporting" or "return_sorting"
                or "returning" or "returned" => ShippingStatus.Returned,
            "cancel" => ShippingStatus.Cancelled,
            _ => null
        };
    }
}
