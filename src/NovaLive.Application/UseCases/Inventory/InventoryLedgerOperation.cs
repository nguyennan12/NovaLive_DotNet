using System.Security.Cryptography;
using System.Text;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Inventory;

public static class InventoryLedgerOperation
{
    // Existing unique OperationId index also protects the deterministic order/SKU transition.
    public static Guid ForOrder(Guid subOrderId, Guid skuId, InventoryChangeType changeType) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"inventory:Order:{subOrderId:N}:{skuId:N}:{changeType}"))
            .AsSpan(0, 16));
}
