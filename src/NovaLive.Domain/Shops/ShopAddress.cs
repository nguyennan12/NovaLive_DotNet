using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopAddress : Entity
{
    private ShopAddress() { }

    public ShopAddress(Guid shopId, string warehouseName, string contactName, string contactPhone,
        int provinceId, string provinceName, int districtId, string districtName,
        string wardCode, string wardName, string detailAddress)
    {
        ShopId = shopId;
        UpdateInfo(warehouseName, contactName, contactPhone, provinceId, provinceName,
            districtId, districtName, wardCode, wardName, detailAddress);
    }

    public void UpdateInfo(string warehouseName, string contactName, string contactPhone,
        int provinceId, string provinceName, int districtId, string districtName,
        string wardCode, string wardName, string detailAddress)
    {
        WarehouseName = warehouseName;
        ContactName = contactName;
        ContactPhone = contactPhone;
        ProvinceId = provinceId;
        ProvinceName = provinceName;
        DistrictId = districtId;
        DistrictName = districtName;
        WardCode = wardCode;
        WardName = wardName;
        DetailAddress = detailAddress;
    }

    public void SetPrimary(bool value) => IsPrimary = value;
    public void SetReturn(bool value) => IsReturn = value;

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
