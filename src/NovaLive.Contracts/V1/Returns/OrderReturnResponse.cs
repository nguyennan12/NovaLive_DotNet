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
    IReadOnlyList<ReturnItemDto> Items,
    IReadOnlyList<string> EvidenceUrls,
    NovaLive.Contracts.V1.Shops.ShopAddressResponse? ReturnAddress = null,
    string? SellerResponse = null,
    IReadOnlyList<string>? SellerEvidenceUrls = null);
