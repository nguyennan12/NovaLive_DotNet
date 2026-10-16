using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Shipping;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;

public sealed class CreateShipmentOrderCommandHandler(
    IParentOrderRepository parents, ISubOrderRepository subs, IShippingOrderRepository shipments,
    IShopAddressRepository addresses, IOrderItemRepository items, ISkuRepository skus,
    IPaymentRepository payments, IOrderStatusHistoryRepository histories, IOutboxRepository outbox,
    ICurrentUser user, ICurrentShop currentShop, OrderShopAccess shopAccess,
    IShippingProviderResolver providers, IDateTimeProvider clock)
    : ICommandHandler<CreateShipmentOrderCommand, ShippingOrderResponse>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<Result<ShippingOrderResponse>> Handle(CreateShipmentOrderCommand command, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } actor || actor == Guid.Empty)
            return ShippingErrors.Unauthorized;
        var access = await shopAccess.GetAsync(ct);
        if (access.IsFailure) return ShippingErrors.SubOrderNotFound;
        var shopId = access.Value;
        var request = command.Request;
        var owned = await subs.FindForShopAsync(request.SubOrderId, shopId, ct);
        if (owned is null || owned.ShopId != currentShop.ShopId) return ShippingErrors.SubOrderNotFound;

        // Same parent lock/order as T23 cancellation and confirmation. A second request
        // waits for commit, then observes the existing shipment before calling the carrier.
        var parent = await parents.GetForUpdateAsync(owned.ParentOrderId, ct);
        if (parent is null) return ShippingErrors.SubOrderNotFound;
        var sub = (await subs.GetByParentIdsAsync([parent.Id], true, ct))
            .SingleOrDefault(s => s.Id == owned.Id && s.ShopId == shopId);
        if (sub is null) return ShippingErrors.SubOrderNotFound;
        if (await shipments.FindBySubOrderIdAsync(sub.Id, ct) is not null)
            return ShippingErrors.ShipmentAlreadyCreated;
        if (sub.Status != SubOrderStatus.Confirmed) return ShippingErrors.InvalidOrderStatus;

        var warehouse = await addresses.FindForShopAsync(request.PickupAddressId, shopId, ct);
        if (warehouse is null || warehouse.ShopId != shopId) return ShippingErrors.InvalidShopWarehouse;
        if (warehouse.DistrictId is not > 0 || string.IsNullOrWhiteSpace(warehouse.WardCode))
            return ShippingErrors.IncompleteWarehouseAddress;

        ShippingAddressSnapshot? recipient;
        try { recipient = JsonSerializer.Deserialize<ShippingAddressSnapshot>(parent.ShippingAddressJson, Json); }
        catch (JsonException) { return ShippingErrors.IncompleteRecipientAddress; }
        if (recipient is null || recipient.DistrictId is not > 0 || string.IsNullOrWhiteSpace(recipient.WardCode)
            || string.IsNullOrWhiteSpace(recipient.RecipientName) || string.IsNullOrWhiteSpace(recipient.Phone)
            || string.IsNullOrWhiteSpace(recipient.DetailAddress))
            return ShippingErrors.IncompleteRecipientAddress;

        var orderItems = await items.GetBySubOrderIdsAsync([sub.Id], ct);
        var skuMap = (await skus.GetByIdsAsync(orderItems.Select(i => i.SkuId), ct)).ToDictionary(s => s.Id);
        var packageItems = new List<ShippingItemModel>();
        long weight = 0;
        foreach (var item in orderItems)
        {
            if (!skuMap.TryGetValue(item.SkuId, out var sku) || sku.ShopId != shopId
                || sku.WeightGram <= 0 || item.Quantity <= 0) return ShippingErrors.InvalidItems;
            weight += (long)sku.WeightGram * item.Quantity;
            ShippingItemSnapshot? snapshot;
            try { snapshot = JsonSerializer.Deserialize<ShippingItemSnapshot>(item.SkuSnapshotJson, Json); }
            catch (JsonException) { return ShippingErrors.InvalidItems; }
            if (snapshot is null || string.IsNullOrWhiteSpace(snapshot.SpuName)
                || string.IsNullOrWhiteSpace(snapshot.SkuCode)) return ShippingErrors.InvalidItems;
            packageItems.Add(new(snapshot.SpuName, snapshot.SkuCode, item.Quantity, sku.WeightGram));
        }
        if (weight <= 0 || weight > int.MaxValue) return ShippingErrors.InvalidItems;
        var payment = await payments.GetByParentOrderIdAsync(parent.Id, ct);
        if (payment is null) return ShippingErrors.PaymentNotFound;
        var cod = payment.Method == PaymentMethod.COD ? sub.SubTotal : 0m;
        var resolved = providers.Resolve(request.Provider);
        if (resolved.IsFailure) return resolved.Error;
        if (!Enum.TryParse<ShippingProvider>(request.Provider, out var provider) || !Enum.IsDefined(provider))
            return ShippingErrors.UnsupportedProvider;
        int? serviceType = null;
        if (provider == ShippingProvider.GHN)
        {
            if (!int.TryParse(request.ServiceCode, out var id) || id <= 0) return ShippingErrors.InvalidServiceCode;
            serviceType = id;
        }
        var model = new ShippingShipmentCreateModel(sub.SubOrderCode,
            warehouse.ContactName, warehouse.ContactPhone, warehouse.DetailAddress, warehouse.WardName,
            warehouse.DistrictName, warehouse.DistrictId.Value, warehouse.WardCode,
            recipient.RecipientName, recipient.Phone, recipient.DetailAddress, recipient.WardCode,
            recipient.DistrictId.Value, cod, (int)weight, "KHONGCHOXEMHANG", packageItems,
            ServiceTypeId: serviceType, ServiceCode: request.ServiceCode);
        Result<ShippingOrderCreateResult> result;
        try { result = await resolved.Value!.CreateShipmentAsync(model, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception)
        {
            return ShippingErrors.ProviderError("Shipping.ProviderError", "Không thể tạo vận đơn với đối tác.");
        }
        if (result.IsFailure) return ShippingErrors.ProviderError("Shipping.ProviderError", result.Error.Message);
        var created = result.Value!;
        if (string.IsNullOrWhiteSpace(created.TrackingCode) || created.TrackingCode.Length > 100
            || string.IsNullOrWhiteSpace(created.ProviderOrderCode) || created.ProviderOrderCode.Length > 100
            || created.TotalFee < 0)
            return ShippingErrors.ProviderError("Shipping.ProviderError", "Đối tác trả về thông tin vận đơn không hợp lệ.");

        // No tracked entity is changed until the carrier has accepted the shipment.
        var now = clock.UtcNow;
        if (sub.WarehouseAddressId is null)
        {
            var assignment = sub.AssignWarehouse(warehouse.Id, now);
            if (assignment.IsFailure) return assignment.Error;
        }
        var transition = sub.MarkAsShipping(now);
        if (transition.IsFailure) return transition.Error;
        var pickupJson = JsonSerializer.Serialize(new
        {
            warehouse.WarehouseName, warehouse.ContactName, warehouse.ContactPhone, warehouse.ProvinceId,
            warehouse.ProvinceName, warehouse.DistrictId, warehouse.DistrictName, warehouse.WardCode,
            warehouse.WardName, warehouse.DetailAddress
        }, Json);
        var shipment = new ShippingOrder(sub.Id, provider, request.ServiceCode, pickupJson,
            parent.ShippingAddressJson, cod, created.TotalFee, (int)weight, created.TrackingCode,
            created.ProviderOrderCode, now, created.ExpectedDeliveryDate);
        await shipments.AddAsync(shipment, ct);
        await histories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.Confirmed,
            SubOrderStatus.Shipping, actor, OrderRole.Seller, "Shipment created", now), ct);
        await outbox.AddAsync(new OutboxMessage(nameof(ShipmentCreatedEvent), JsonSerializer.Serialize(
            new ShipmentCreatedEvent(shipment.Id, sub.Id, shopId, request.Provider,
                created.TrackingCode, created.TotalFee, now), Json), now), ct);
        return new ShippingOrderResponse(shipment.Id, sub.Id, request.Provider, created.TrackingCode,
            created.TotalFee, shipment.Status.ToString(), now.UtcDateTime);
    }
}
