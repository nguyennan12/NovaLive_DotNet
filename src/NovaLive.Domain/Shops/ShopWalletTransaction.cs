using NovaLive.Domain.Common;

namespace NovaLive.Domain.Shops;

public sealed class ShopWalletTransaction : Entity
{
    public Guid WalletId { get; private set; }

    public WalletTxType Type { get; private set; }

    public decimal Amount { get; private set; }

    public decimal BalanceBefore { get; private set; }

    public decimal BalanceAfter { get; private set; }

    public string? RefType { get; private set; }

    public Guid? RefId { get; private set; }

    public string? Description { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
