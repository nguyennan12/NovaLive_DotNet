using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopAddress : Entity
{
    public Guid ShopId { get; private set; }

    public string WarehouseName { get; private set; } = string.Empty;

    public string ContactName { get; private set; } = string.Empty;

    public string ContactPhone { get; private set; } = string.Empty;

    public int? ProvinceId { get; private set; }

    public string ProvinceName { get; private set; } = string.Empty;

    public int? DistrictId { get; private set; }

    public string DistrictName { get; private set; } = string.Empty;

    public string? WardCode { get; private set; }

    public string WardName { get; private set; } = string.Empty;

    public string DetailAddress { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public bool IsReturn { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
