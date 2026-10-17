namespace NovaLive.Contracts.V1.Returns;

public record ReturnItemDto(
    Guid OrderItemId,
    string SpuName,
    string SkuCode,
    int Quantity,
    decimal RefundPrice);
