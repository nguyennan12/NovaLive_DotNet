namespace NovaLive.Contracts.V1.Shops;

public record RegisterShopRequest(
    string ShopName,
    string Description,
    string Phone,
    string Email,
    string KycFrontUrl,
    string KycBackUrl,
    string BankAccount,
    string BankName,
    string BankHolder);

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
    bool IsReturn);

public record ApproveShopRequest(
    string? Note);

public record RejectShopRequest(
    string Reason);

public record BanShopRequest(
    string Reason);
