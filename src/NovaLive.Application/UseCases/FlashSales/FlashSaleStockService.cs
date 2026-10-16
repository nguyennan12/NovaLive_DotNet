using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.FlashSales;

public sealed class FlashSaleStockService(IOrderItemRepository orders, IFlashSaleRepository flashSales) : IFlashSaleStockService
{
    public Task<Result> ConfirmSoldAsync(IEnumerable<Guid> subOrderIds, CancellationToken ct = default) =>
        TransitionAsync(subOrderIds, FlashSaleReservationState.Sold, ct);

    public Task<Result> ReleaseReservedAsync(IEnumerable<Guid> subOrderIds, CancellationToken ct = default) =>
        TransitionAsync(subOrderIds, FlashSaleReservationState.Released, ct);

    private async Task<Result> TransitionAsync(IEnumerable<Guid> subOrderIds, FlashSaleReservationState target, CancellationToken ct)
    {
        var lines = await orders.GetBySubOrderIdsAsync(subOrderIds, ct);
        foreach (var line in lines.Where(i => i.FlashSaleItemId.HasValue).OrderBy(i => i.FlashSaleItemId).ThenBy(i => i.Id))
        {
            // The conditional state transition serializes duplicate or competing deliveries.
            if (!await orders.TryTransitionFlashSaleAsync(line.Id, target, ct))
            {
                if (await orders.GetFlashSaleStateAsync(line.Id, ct) == target) continue;
                return FlashSalesErrors.ReservationConflict;
            }
            var changed = target == FlashSaleReservationState.Sold
                ? await flashSales.ConfirmSoldAsync(line.FlashSaleItemId!.Value, line.Quantity, ct)
                : await flashSales.ReleaseReservedAsync(line.FlashSaleItemId!.Value, line.Quantity, ct);
            if (!changed) return FlashSalesErrors.ReservationConflict;
        }
        return Result.Success();
    }
}
