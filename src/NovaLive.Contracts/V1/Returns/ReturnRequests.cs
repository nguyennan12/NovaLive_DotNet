namespace NovaLive.Contracts.V1.Returns;

public record CreateReturnRequest(
    Guid SubOrderId,
    string Reason,
    List<ReturnItemInputDto> Items,
    List<string> EvidenceUrls);

public record ReturnItemInputDto(
    Guid OrderItemId,
    int Quantity,
    string? ReasonDetail);

public record SellerReturnResponseRequest(
    string? ReasonNote);

public record ResolveDisputeRequest(
    string Winner,
    decimal? RefundAmount,
    string AdminNote);
