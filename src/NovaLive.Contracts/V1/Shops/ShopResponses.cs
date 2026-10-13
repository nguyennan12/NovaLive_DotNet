namespace NovaLive.Contracts.V1.Shops;

public record ShopResponse(
    Guid Id,
    Guid OwnerId,
    string ShopName,
    string Slug,
    string Description,
    string? LogoUrl,
    string? BannerUrl,
    string Phone,
    string Email,
    string Status,
    double Rating,
    int FollowersCount,
    DateTime CreatedAt,
    string? VerificationStatus = null,
    string? ReviewerNote = null,
    string? BankAccount = null);

public record ShopDetailResponse(
    Guid Id,
    Guid OwnerId,
    string ShopName,
    string Slug,
    string Description,
    string? LogoUrl,
    string? BannerUrl,
    string Phone,
    string Email,
    string Status,
    double Rating,
    int FollowersCount,
    DateTime CreatedAt,
    List<ShopAddressResponse> Warehouses,
    int RatingCount = 0,
    int ProductsCount = 0,
    bool IsFollowing = false);

public record ShopAddressResponse(
    Guid Id,
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
