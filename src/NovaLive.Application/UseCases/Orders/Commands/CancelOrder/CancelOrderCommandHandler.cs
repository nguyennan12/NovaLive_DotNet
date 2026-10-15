using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.System;
namespace NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
public sealed class CancelOrderCommandHandler(IParentOrderRepository parents, ISubOrderRepository subs,
    IPaymentRepository payments, IOrderStatusHistoryRepository histories, IOutboxRepository outbox,
    OrderReservationReleaser releaser, ICurrentUser user, IDateTimeProvider clock)
    : ICommandHandler<CancelOrderCommand, OrderOperationResponse>
{
    public async Task<Result<OrderOperationResponse>> Handle(CancelOrderCommand command, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } buyer || buyer == Guid.Empty) return CheckoutErrors.Unauthorized;
        var parent = await parents.GetForUpdateAsync(command.OrderId, ct);
        if (parent is null || parent.BuyerId != buyer) return OrderErrors.OrderNotFound;
        if (parent.PaymentStatus == ParentOrderPaymentStatus.Cancelled) return OrderStateErrors.AlreadyCancelled;
        var children = await subs.GetByParentIdsAsync([parent.Id], true, ct);
        if (children.Count == 0 || children.Any(s => s.Status != SubOrderStatus.PendingPayment))
            return OrderStateErrors.CannotCancelConfirmedOrder;
        var payment = await payments.GetForUpdateAsync(parent.Id, ct);
        if (payment?.Status is PaymentStatus.Success or PaymentStatus.Refunded || parent.PaymentStatus != ParentOrderPaymentStatus.Pending)
            return OrderStateErrors.CannotCancelPaidOrderYet;
        var now = clock.UtcNow;
        foreach (var sub in children)
        {
            var cancelled = sub.Cancel(buyer, OrderRole.Buyer, command.Request.Reason, now);
            if (cancelled.IsFailure) return cancelled.Error;
        }
        var release = await releaser.ReleaseAsync(parent, children, buyer, ct);
        if (release.IsFailure) return release.Error;
        var result = parent.CancelPayment(now);
        if (result.IsFailure) return result.Error;
        payment?.ExpirePending(now);
        foreach (var sub in children)
            await histories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.PendingPayment, SubOrderStatus.Cancelled,
                buyer, OrderRole.Buyer, command.Request.Reason, now), ct);
        await outbox.AddAsync(new OutboxMessage("OrderCancelledEvent", JsonSerializer.Serialize(new
        {
            parentOrderId = parent.Id, subOrderIds = children.Select(s => s.Id), buyerId = buyer,
            changedByRole = "Buyer", reason = command.Request.Reason, cancelledAt = now
        })), ct);
        return new OrderOperationResponse(parent.Id, parent.PaymentStatus.ToString());
    }
}
