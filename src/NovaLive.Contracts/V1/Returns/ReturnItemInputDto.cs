namespace NovaLive.Contracts.V1.Returns;

public record ReturnItemInputDto(
    Guid OrderItemId,
    int Quantity,
    string? ReasonDetail);
