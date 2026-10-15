using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.System;
namespace NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
public sealed class CancelSubOrderCommandHandler(IParentOrderRepository parents, ISubOrderRepository subs,
    IPaymentRepository payments, IOrderStatusHistoryRepository histories, IOutboxRepository outbox,
    OrderReservationReleaser releaser, ICurrentUser user, OrderShopAccess shopAccess, IDateTimeProvider clock)
    : ICommandHandler<CancelSubOrderCommand, OrderOperationResponse>
{
    public async Task<Result<OrderOperationResponse>> Handle(CancelSubOrderCommand command, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } actor || actor == Guid.Empty) return CheckoutErrors.Unauthorized;
        var context = await shopAccess.GetAsync(ct);
        if (context.IsFailure) return context.Error;
        var shopId = context.Value;
        var owned = await subs.FindForShopAsync(command.SubOrderId, shopId, ct);
        if (owned is null || owned.ShopId != shopId) return OrderErrors.SubOrderNotFound;
        var parent = await parents.GetForUpdateAsync(owned.ParentOrderId, ct);
        if (parent is null) return OrderErrors.SubOrderNotFound;
        var children = await subs.GetByParentIdsAsync([parent.Id], true, ct);
        var sub = children.SingleOrDefault(s => s.Id == command.SubOrderId && s.ShopId == shopId);
        if (sub is null) return OrderErrors.SubOrderNotFound;
        if (sub.Status == SubOrderStatus.Cancelled) return OrderStateErrors.AlreadyCancelled;
        var payment = await payments.GetForUpdateAsync(parent.Id, ct);
        // TODO T32/T38/T39: paid orders require refund and wallet reversal, never reservation release alone.
        if (payment?.Status is PaymentStatus.Success or PaymentStatus.Refunded || parent.PaymentStatus != ParentOrderPaymentStatus.Pending)
            return OrderStateErrors.CannotCancelPaidOrderYet;
        var now = clock.UtcNow; var from = sub.Status;
        var cancelled = sub.Cancel(actor, OrderRole.Seller, command.Request.Reason, now);
        if (cancelled.IsFailure) return cancelled.Error;
        var released = await releaser.ReleaseAsync(parent, [sub], actor, ct);
        if (released.IsFailure) return released.Error;
        if (children.All(s => s.Status == SubOrderStatus.Cancelled))
        {
            var result = parent.CancelPayment(now);
            if (result.IsFailure) return result.Error;
            payment?.ExpirePending(now);
        }
        await histories.AddAsync(OrderStatusHistory.Create(sub.Id, from, SubOrderStatus.Cancelled,
            actor, OrderRole.Seller, command.Request.Reason, now), ct);
        await outbox.AddAsync(new OutboxMessage("SubOrderCancelledEvent", JsonSerializer.Serialize(new
        {
            parentOrderId = parent.Id, subOrderId = sub.Id, shopId, changedBy = actor,
            changedByRole = "Seller", reason = command.Request.Reason, cancelledAt = now
        })), ct);
        return new OrderOperationResponse(sub.Id, sub.Status.ToString());
    }
}
