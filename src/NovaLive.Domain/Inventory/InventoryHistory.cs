using NovaLive.Domain.Common;

namespace NovaLive.Domain.Inventory;

public sealed class InventoryHistory : Entity
{
    public Guid InventoryId { get; set; }

    public Guid SkuId { get; set; }

    public InventoryChangeType ChangeType { get; set; }

    public int QtyBefore { get; set; }

    public int QtyChange { get; set; }

    public int ReservedBefore { get; set; }

    public int ReservedChange { get; set; }

    public int QtyAfter { get; set; }

    public string? RefType { get; set; }

    public Guid? RefId { get; set; }

    public string? Note { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
