using NovaLive.Domain.Common;

namespace NovaLive.Application.Abstractions.Services;

/// <summary>
/// Call inside the order/payment transaction and propagate failure to roll back the entire operation.
/// Quantities and Flash Sale IDs are loaded from persisted order lines, never supplied by the event.
/// </summary>
public interface IFlashSaleStockService
{
    Task<Result> ConfirmSoldAsync(IEnumerable<Guid> subOrderIds, CancellationToken ct = default);
    Task<Result> ReleaseReservedAsync(IEnumerable<Guid> subOrderIds, CancellationToken ct = default);
}
