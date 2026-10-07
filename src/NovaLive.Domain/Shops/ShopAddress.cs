using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopAddress : Entity
{
    public Guid ShopId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public int? ProvinceId { get; set; }

    public string ProvinceName { get; set; } = string.Empty;

    public int? DistrictId { get; set; }

    public string DistrictName { get; set; } = string.Empty;

    public string? WardCode { get; set; }

    public string WardName { get; set; } = string.Empty;

    public string DetailAddress { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsReturn { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
