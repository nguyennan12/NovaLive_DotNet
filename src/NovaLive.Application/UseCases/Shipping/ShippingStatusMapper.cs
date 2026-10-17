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
            "ready_for_pick" => ShippingStatus.ReadyToPick,
            "storing" => ShippingStatus.Picking,
            "delivery_fail" => ShippingStatus.Failed,
            "return" or "returning" => ShippingStatus.Returned,
            "cancel" => ShippingStatus.Cancelled,
            _ => null
        };
    }
}
