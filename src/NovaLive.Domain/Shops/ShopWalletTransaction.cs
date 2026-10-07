using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopWalletTransaction : Entity
{
    public Guid WalletId { get; set; }

    public WalletTxType Type { get; set; }

    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public string? RefType { get; set; }

    public Guid? RefId { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
