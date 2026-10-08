namespace NovaLive.Contracts.V1.Shops;

public record CreatePayoutRequest(
    decimal Amount,
    string BankName,
    string BankAccount,
    string BankHolder);

public record ApprovePayoutRequest(
    string? TransferRef);

public record RejectPayoutRequest(
    string Reason);
