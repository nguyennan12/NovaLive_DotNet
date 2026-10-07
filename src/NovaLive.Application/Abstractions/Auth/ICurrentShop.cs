namespace NovaLive.Application.Abstractions.Auth;

public interface ICurrentShop
{
    Guid? ShopId { get; }

    bool HasShopContext => ShopId.HasValue && ShopId.Value != Guid.Empty;
}
