namespace NovaLive.Contracts.V1.Shops;

public record RegisterShopRequest(
    string ShopName,
    string Description,
    string Phone,
    string Email,
    string IdCardFront,
    string IdCardBack,
    string BankAccount,
    string BankName,
    string? Slug = null,
    string? TaxCode = null,
    string? BankBranch = null,
    string? BusinessLicense = null);

public record UpdateShopProfileRequest(
    string ShopName,
    string Description,
    string? LogoUrl,
    string? BannerUrl,
    string Phone,
    string Email);

public record CreateShopAddressRequest(
    string WarehouseName,
    string ContactName,
    string ContactPhone,
    string ProvinceName,
    string DistrictName,
    string WardName,
    string DetailAddress,
    bool IsPrimary,
    bool IsReturn,
    int? ProvinceId = null,
    int? DistrictId = null,
    string? WardCode = null);

public record ApproveShopRequest(
    string? Note);

public record RejectShopRequest(
    string Reason);

public record BanShopRequest(
    string Reason);
