namespace NovaLive.Contracts.V1.Returns;

public record OrderReturnResponse(
    Guid Id,
    Guid SubOrderId,
    string SubOrderCode,
    Guid BuyerId,
    string Reason,
    string Status,
    decimal RequestedRefundAmount,
    decimal? ApprovedRefundAmount,
    DateTime CreatedAt,
    List<ReturnItemDto> Items,
    List<string> EvidenceUrls);

public record ReturnItemDto(
    Guid OrderItemId,
    string SpuName,
    string SkuCode,
    int Quantity,
    decimal RefundPrice);
