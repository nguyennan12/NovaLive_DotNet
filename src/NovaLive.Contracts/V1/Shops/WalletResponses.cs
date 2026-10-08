namespace NovaLive.Contracts.V1.Shops;

public record ShopWalletResponse(
    Guid ShopId,
    decimal Balance,
    decimal HoldingBalance,
    decimal LockedBalance,
    DateTime UpdatedAt);

public record WalletTransactionResponse(
    Guid Id,
    Guid WalletId,
    decimal Amount,
    string Type,
    string Description,
    decimal BalanceAfter,
    DateTime CreatedAt);

public record SellerPayoutResponse(
    Guid Id,
    Guid ShopId,
    decimal Amount,
    string BankName,
    string BankAccount,
    string BankHolder,
    string Status,
    string? TransferRef,
    string? Reason,
    DateTime CreatedAt);
