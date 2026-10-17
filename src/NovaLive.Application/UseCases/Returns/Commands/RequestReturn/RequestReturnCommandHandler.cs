using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.UseCases.Returns.Commands.RequestReturn;
public sealed class RequestReturnCommandHandler(IParentOrderRepository parents, ISubOrderRepository subs,
    IOrderItemRepository items, IOrderReturnRepository returns, IOrderReturnItemRepository returnItems,
    IOrderStatusHistoryRepository histories, ICurrentUser user, IDateTimeProvider clock)
    : ICommandHandler<RequestReturnCommand, OrderReturnResponse>
{
    public async Task<Result<OrderReturnResponse>> Handle(RequestReturnCommand command, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } buyer) return ReturnErrors.Unauthorized;
        var request = command.Request;
        var owned = await subs.FindByIdAsync(request.SubOrderId, ct);
        if (owned is null) return ReturnErrors.SubOrderNotFound;
        // Same parent/sub-order lock order as fulfillment and cancellation. Prevent duplicate requests.
        var parent = await parents.GetForUpdateAsync(owned.ParentOrderId, ct);
        if (parent?.BuyerId != buyer) return ReturnErrors.SubOrderNotFound;
        var sub = await subs.GetForUpdateAsync(owned.Id, ct);
        if (sub is null) return ReturnErrors.SubOrderNotFound;
        var previous = await returns.GetBySubOrderIdAsync(sub.Id, ct);
        if (previous.Any(r => r.Status is ReturnStatus.Pending or ReturnStatus.SellerApproved or ReturnStatus.AdminDispute))
            return ReturnErrors.ActiveReturnExists;
        if (sub.Status != SubOrderStatus.Delivered || sub.DeliveredAt is null) return ReturnErrors.NotDelivered;
        var now = clock.UtcNow;
        if (now > sub.DeliveredAt.Value.AddDays(7)) return ReturnErrors.Expired;
        var purchased = (await items.GetBySubOrderIdsAsync([sub.Id], ct)).ToDictionary(i => i.Id);
        var countedIds = previous.Where(r => r.Status is not (ReturnStatus.SellerRejected or ReturnStatus.AdminRejected)).Select(r => r.Id);
        var previousItems = await returnItems.GetByReturnIdsAsync(countedIds, ct);
        decimal refund = 0;
        foreach (var input in request.Items)
        {
            if (!purchased.TryGetValue(input.OrderItemId, out var item) || item.SubOrderId != sub.Id)
                return ReturnErrors.ItemNotInSubOrder;
            var used = previousItems.Where(i => i.OrderItemId == item.Id).Sum(i => (long)i.Quantity);
            var amount = ReturnRefundCalculator.Calculate(item, input.Quantity, used);
            if (amount.IsFailure) return amount.Error;
            refund += amount.Value;
        }
        var transition = sub.RequestReturn(now);
        if (transition.IsFailure) return transition.Error;
        var entity = OrderReturn.Create(sub.Id, buyer, Enum.Parse<ReturnReason>(request.Reason), request.EvidenceUrls, refund, now);
        await returns.AddAsync(entity, ct);
        var lines = request.Items.Select(i => OrderReturnItem.Create(entity.Id, i.OrderItemId, i.Quantity, i.ReasonDetail)).ToArray();
        foreach (var line in lines) await returnItems.AddAsync(line, ct);
        await histories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.Delivered, SubOrderStatus.ReturnRequested,
            buyer, OrderRole.Buyer, "Return requested", now), ct);
        return ReturnMappings.ToResponse(entity, sub, lines, purchased.Values);
    }
}
