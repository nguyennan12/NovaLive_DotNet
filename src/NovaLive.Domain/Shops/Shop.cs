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

    public void Approve()
    {
        Status = ShopStatus.Active;
    }

    public void Suspend()
    {
        Status = ShopStatus.Suspended;
    }
}
