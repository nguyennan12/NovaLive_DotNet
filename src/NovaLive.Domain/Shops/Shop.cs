using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class Shop : AuditableEntity
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

    public Guid OwnerId { get; set; }

    public string ShopName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public string? TaxCode { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public decimal RatingAvg { get; set; } = 0.00m;

    public int RatingCount { get; set; } = 0;

    public ShopStatus Status { get; set; } = ShopStatus.Pending;

    public DateTimeOffset? DeletedAt { get; set; }

    public void Approve()
    {
        Status = ShopStatus.Active;
    }

    public void Suspend()
    {
        Status = ShopStatus.Suspended;
    }
}
