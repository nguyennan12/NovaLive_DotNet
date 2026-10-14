using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class Shop : AuditableEntity, ISoftDeletable
{
    private Shop()
    {
    }

    public Shop(Guid ownerId, string shopName, string slug)
    {
        OwnerId = ownerId;
        ShopName = shopName;
        Slug = slug;
    }

    public Guid OwnerId { get; private set; }

    public string ShopName { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? LogoUrl { get; private set; }

    public string? BannerUrl { get; private set; }

    public string? TaxCode { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public decimal RatingAvg { get; private set; } = 0.00m;

    public int RatingCount { get; private set; } = 0;

    public ShopStatus Status { get; private set; } = ShopStatus.Pending;

    public DateTimeOffset? DeletedAt { get; private set; }

    public bool CanManage => DeletedAt is null && Status == ShopStatus.Active;

    public static Shop Register(Guid ownerId, string name, string slug, string description,
        string phone, string email, string? taxCode)
    {
        return new Shop(ownerId, name, slug)
        {
            TaxCode = taxCode,
            Description = description,
            Phone = phone,
            Email = email
        };
    }

    public Result UpdateInfo(string name, string description, string? logoUrl, string? bannerUrl,
        string phone, string email)
    {
        if (!CanManage)
            return Error.Forbidden("Shops.CannotManage", "Gian hàng hiện không cho phép chỉnh sửa.");
        ShopName = name;
        Description = description;
        LogoUrl = logoUrl;
        BannerUrl = bannerUrl;
        Phone = phone;
        Email = email;
        return Result.Success();
    }

    public void Approve()
    {
        Status = ShopStatus.Active;
    }

    public void Suspend()
    {
        Status = ShopStatus.Suspended;
    }
}
