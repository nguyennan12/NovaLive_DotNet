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
    DateTime CreatedAt);

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
    List<ShopAddressResponse> Warehouses);

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
    bool IsReturn);
