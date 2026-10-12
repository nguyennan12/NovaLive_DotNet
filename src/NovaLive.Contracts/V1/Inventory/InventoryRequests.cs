using NovaLive.Contracts.Common;

namespace NovaLive.Contracts.V1.Inventory;

public record AdjustInventoryRequest(
    Guid SkuId,
    int QtyChange,
    string ChangeType,
    string? Note = null,
    Guid? OperationId = null);

public record GetSellerInventoryRequest(
    bool? LowStock = null,
    string? Keyword = null,
    int Page = 1,
    int Size = 20) : PagedRequest(Page, Size);

public record GetInventoryHistoriesRequest(
    Guid? SkuId = null,
    string? ChangeType = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    int Page = 1,
    int Size = 20) : PagedRequest(Page, Size);
