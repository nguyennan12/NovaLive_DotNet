using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
public sealed class ConfirmSubOrderCommandHandler(IParentOrderRepository parents, ISubOrderRepository subs,
    IShopAddressRepository addresses, IOrderStatusHistoryRepository histories,
    ICurrentUser user, OrderShopAccess shopAccess, IDateTimeProvider clock)
    : ICommandHandler<ConfirmSubOrderCommand, OrderOperationResponse>
{
    public async Task<Result<OrderOperationResponse>> Handle(ConfirmSubOrderCommand command, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } actor || actor == Guid.Empty) return CheckoutErrors.Unauthorized;
        var context = await shopAccess.GetAsync(ct);
        if (context.IsFailure) return context.Error;
        var shopId = context.Value;
        var owned = await subs.FindForShopAsync(command.SubOrderId, shopId, ct);
        if (owned is null || owned.ShopId != shopId) return OrderErrors.SubOrderNotFound;
        if (await parents.GetForUpdateAsync(owned.ParentOrderId, ct) is null) return OrderErrors.SubOrderNotFound;
        var sub = (await subs.GetByParentIdsAsync([owned.ParentOrderId], true, ct))
            .SingleOrDefault(s => s.Id == command.SubOrderId && s.ShopId == shopId);
        if (sub is null) return OrderErrors.SubOrderNotFound;
        var warehouse = await addresses.FindForShopAsync(command.Request.WarehouseAddressId, shopId, ct);
        if (warehouse is null || warehouse.ShopId != shopId) return OrderErrors.WarehouseNotFound;
        var now = clock.UtcNow;
        var assigned = sub.AssignWarehouse(warehouse.Id, now);
        if (assigned.IsFailure) return assigned.Error;
        await histories.AddAsync(OrderStatusHistory.Create(sub.Id, sub.Status, sub.Status, actor, OrderRole.Seller,
            $"Xác nhận kho lấy hàng: {warehouse.Id}", now), ct);
        return new OrderOperationResponse(sub.Id, sub.Status.ToString());
    }
}
