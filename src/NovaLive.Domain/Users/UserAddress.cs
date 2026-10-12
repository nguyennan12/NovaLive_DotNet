using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class UserAddress : AuditableEntity
{
    private UserAddress()
    {
    }

    public UserAddress(
        Guid userId,
        string recipientName,
        string phone,
        int? provinceId,
        string provinceName,
        int? districtId,
        string districtName,
        string? wardCode,
        string wardName,
        string detailAddress,
        bool isDefault = false)
    {
        UserId = userId;
        RecipientName = recipientName;
        Phone = phone;
        ProvinceId = provinceId;
        ProvinceName = provinceName;
        DistrictId = districtId;
        DistrictName = districtName;
        WardCode = wardCode;
        WardName = wardName;
        DetailAddress = detailAddress;
        IsDefault = isDefault;
    }

    public Guid UserId { get; private set; }

    public string RecipientName { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public int? ProvinceId { get; private set; }

    public string ProvinceName { get; private set; } = string.Empty;

    public int? DistrictId { get; private set; }

    public string DistrictName { get; private set; } = string.Empty;

    public string? WardCode { get; private set; }

    public string WardName { get; private set; } = string.Empty;

    public string DetailAddress { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }

    public void UpdateDetails(
        string recipientName,
        string phone,
        int? provinceId,
        string provinceName,
        int? districtId,
        string districtName,
        string? wardCode,
        string wardName,
        string detailAddress,
        bool isDefault)
    {
        RecipientName = recipientName;
        Phone = phone;
        ProvinceId = provinceId;
        ProvinceName = provinceName;
        DistrictId = districtId;
        DistrictName = districtName;
        WardCode = wardCode;
        WardName = wardName;
        DetailAddress = detailAddress;
        IsDefault = isDefault;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
    }

    public void UnsetDefault()
    {
        IsDefault = false;
    }
}
