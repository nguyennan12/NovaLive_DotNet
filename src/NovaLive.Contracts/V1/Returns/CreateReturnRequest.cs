namespace NovaLive.Contracts.V1.Returns;

public record CreateReturnRequest(
    Guid SubOrderId,
    string Reason,
    IReadOnlyList<ReturnItemInputDto> Items,
    IReadOnlyList<string> EvidenceUrls);
