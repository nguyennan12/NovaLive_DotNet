using NovaLive.Contracts.Common;

namespace NovaLive.Contracts.V1.Shops;

public record CreatePayoutRequest(
    decimal Amount,
    string? BankName = null,
    string? BankAccount = null,
    string? BankHolder = null);

public record PayoutRequest(
    decimal Amount,
    string? BankName = null,
    string? BankAccount = null,
    string? BankHolder = null);

public record GetWalletTransactionsRequest(
    string? Type = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int Size = 20) : PagedRequest(Page, Size);

public record ApprovePayoutRequest(
    string? TransferRef);

public record RejectPayoutRequest(
    string Reason);
