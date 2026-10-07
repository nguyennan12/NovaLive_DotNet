using NovaLive.Domain.Common;

namespace NovaLive.Domain.Inventory;

public sealed class InventoryHistory : Entity
{
    public Guid InventoryId { get; private set; }

    public Guid SkuId { get; private set; }

    public InventoryChangeType ChangeType { get; private set; }

    public int QtyBefore { get; private set; }

    public int QtyChange { get; private set; }

    public int ReservedBefore { get; private set; }

    public int ReservedChange { get; private set; }

    public int QtyAfter { get; private set; }

    public string? RefType { get; private set; }

    public Guid? RefId { get; private set; }

    public string? Note { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
}
