using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.UseCases.Shops;

public static class ShopMappings
{
    public static ShopResponse ToResponse(this Shop shop, int followers, ShopVerification? verification = null) =>
        new(shop.Id, shop.OwnerId, shop.ShopName, shop.Slug, shop.Description ?? "", shop.LogoUrl,
            shop.BannerUrl, shop.Phone ?? "", shop.Email ?? "", shop.Status.ToString(), (double)shop.RatingAvg,
            followers, shop.CreatedAt.UtcDateTime, verification?.Status.ToString(), verification?.ReviewerNote,
            verification is null ? null : MaskAccount(verification.BankAccount));

    private static string MaskAccount(string account) => account.Length <= 4
        ? new string('*', account.Length) : "******" + account[^4..];

    public static ShopAddressResponse ToResponse(this ShopAddress address) =>
        new(address.Id, address.WarehouseName, address.ContactName, address.ContactPhone,
            address.ProvinceName, address.DistrictName, address.WardName, address.DetailAddress,
            address.IsPrimary, address.IsReturn, address.ProvinceId, address.DistrictId, address.WardCode);
}
