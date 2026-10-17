using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.UseCases.Returns;
public sealed class ReturnReader(ISubOrderRepository subs, IOrderReturnItemRepository returnItems, IOrderItemRepository items)
{
    public async Task<OrderReturnResponse?> DetailAsync(OrderReturn request, CancellationToken ct)
    {
        var sub = await subs.FindByIdAsync(request.SubOrderId, ct);
        if (sub is null) return null;
        return ReturnMappings.ToResponse(request, sub, await returnItems.GetByReturnIdsAsync([request.Id], ct),
            await items.GetBySubOrderIdsAsync([sub.Id], ct));
    }
    public async Task<PagedResult<OrderReturnResponse>> PageAsync(List<OrderReturn> page, long total, int number, int size, CancellationToken ct)
    {
        var subsById = (await subs.GetByIdsAsync(page.Select(r => r.SubOrderId), ct)).ToDictionary(s => s.Id);
        var lines = await returnItems.GetByReturnIdsAsync(page.Select(r => r.Id), ct);
        var purchased = await items.GetBySubOrderIdsAsync(subsById.Keys, ct);
        return new(page.Where(r => subsById.ContainsKey(r.SubOrderId)).Select(r => ReturnMappings.ToResponse(r,
            subsById[r.SubOrderId], lines, purchased.Where(i => i.SubOrderId == r.SubOrderId))).ToArray(), number, size, total);
    }
}
