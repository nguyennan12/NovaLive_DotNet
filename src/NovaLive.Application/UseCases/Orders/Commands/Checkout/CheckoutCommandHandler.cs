using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Discounts;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.System;
namespace NovaLive.Application.UseCases.Orders.Commands.Checkout;
public sealed class CheckoutCommandHandler(CheckoutDraftBuilder builder, ITransactionManager transactions,
    ICartRepository carts, IInventoryRepository inventories, IParentOrderRepository parents,
    ISubOrderRepository subs, IOrderItemRepository orderItems, IOrderStatusHistoryRepository histories,
    IPaymentRepository payments, IDiscountRepository discounts, IOutboxRepository outbox,
    ICurrentUser user, IDateTimeProvider clock) : ICommandHandler<CheckoutCommand, CheckoutResponse>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task<Result<CheckoutResponse>> Handle(CheckoutCommand command, CancellationToken ct)
    {
        if (!user.IsAuthenticated || user.UserId is not { } buyer || buyer == Guid.Empty) return CheckoutErrors.Unauthorized;
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"checkout:{buyer}:{command.IdempotencyKey}")));
        var receipt = await outbox.FindCheckoutReceiptAsync(key, ct);
        if (receipt is not null) return receipt;
        var request = command.Request;
        var prepared = await builder.BuildAsync(new CalculateCheckoutRequest(request.CartItemIds.ToList(),
            request.ShippingAddressId, request.ShopVouchers?.ToList(), request.PlatformProductVoucherCode,
            request.PlatformFreeshipVoucherCode), ct, request.ShippingProviders);
        if (prepared.IsFailure)
        {
            // A concurrent request may have committed and removed the cart during preparation.
            receipt = await outbox.FindCheckoutReceiptAsync(key, ct);
            return receipt is not null ? receipt : prepared.Error;
        }
        var draft = prepared.Value!;
        if (draft.Price.GrandTotal <= 0) return OrderErrors.ZeroAmountNotAllowed;
        return await transactions.ExecuteAsync<Result<CheckoutResponse>>(async () =>
        {
            await carts.LockUserAsync(buyer, ct);
            receipt = await outbox.FindCheckoutReceiptAsync(key, ct);
            if (receipt is not null) return receipt;
            var selected = await carts.GetCartItemsByIdsAsync(request.CartItemIds, buyer, ct);
            if (selected.Count != draft.Items.Count || selected.Any(i => !draft.Items.Any(d =>
                d.Id == i.Id && d.CartId == i.CartId && d.SkuId == i.SkuId && d.ShopId == i.ShopId && d.Quantity == i.Quantity)))
                return OrderErrors.CartChanged;
            var quantities = draft.Items.GroupBy(i => i.SkuId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
            var stock = (await inventories.GetByIdsForUpdateAsync(quantities.Keys.Order(), ct)).ToDictionary(i => i.SkuId);
            foreach (var (sku, qty) in quantities)
                if (!stock.TryGetValue(sku, out var inv) || inv.ShopId != draft.Skus[sku].ShopId || inv.AvailableQty < qty)
                    return CheckoutErrors.InsufficientStock(sku);
            var now = clock.UtcNow;
            var price = draft.Price;
            ParentOrder? parent = null;
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var a = draft.Address;
                var address = JsonSerializer.Serialize(new { a.RecipientName, a.Phone, a.ProvinceId, a.ProvinceName,
                    a.DistrictId, a.DistrictName, a.WardCode, a.WardName, a.DetailAddress }, Json);
                var candidate = ParentOrder.Create(buyer, OrderCodeGenerator.Generate(now), address, price.TotalItemAmount,
                    price.TotalShippingFee, price.TotalShopDiscount + price.TotalPlatformDiscount + price.TotalShippingDiscount,
                    price.GrandTotal, request.Note, now);
                // Insert before ledger mutations so a unique-code retry only detaches the failed parent.
                // All writes, including this flush, remain within the same outer transaction.
                if (await parents.TryAddAsync(candidate, ct)) { parent = candidate; break; }
            }
            if (parent is null) return OrderErrors.CodeUnavailable;
            foreach (var (sku, qty) in quantities.OrderBy(q => q.Key))
            {
                var inv = stock[sku]; var before = inv.ReservedQty;
                var reservation = inv.TryReserve(qty);
                if (reservation.IsFailure) return reservation.Error;
                await inventories.AddHistoryAsync(new InventoryHistory(inv.Id, sku, InventoryChangeType.ReserveAdd,
                    inv.QtyOnHand, 0, before, qty, inv.QtyOnHand, refType: "Order", refId: parent.Id, createdBy: buyer), ct);
            }
            var method = Enum.Parse<PaymentMethod>(request.PaymentMethod);
            var responseSubs = new List<CheckoutSubOrderResponse>();
            var eventSubs = new List<OrderPlacedShop>();
            foreach (var shop in price.ShopResults.OrderBy(s => s.ShopId))
            {
                var sub = SubOrder.Create(parent.Id, $"{parent.OrderCode}-S{responseSubs.Count + 1}",
                    draft.Warehouses[shop.ShopId].Id, shop, method == PaymentMethod.COD, now);
                await subs.AddAsync(sub, ct);
                await histories.AddAsync(OrderStatusHistory.Create(sub.Id, null, SubOrderStatus.PendingPayment,
                    buyer, OrderRole.Buyer, "Checkout", now), ct);
                if (method == PaymentMethod.COD)
                    await histories.AddAsync(OrderStatusHistory.Create(sub.Id, SubOrderStatus.PendingPayment,
                        SubOrderStatus.Confirmed, buyer, OrderRole.System, "COD confirmed", now), ct);
                foreach (var line in shop.Items)
                {
                    var sku = draft.Skus[line.SkuId]; var spu = draft.Spus[line.SpuId];
                    using var attributes = JsonDocument.Parse(sku.AttributesJson ?? "{}");
                    var snapshot = JsonSerializer.Serialize(new { sku.SkuCode, SpuName = spu.Name,
                        Attributes = attributes.RootElement, spu.ThumbnailUrl }, Json);
                    await orderItems.AddAsync(OrderItem.Create(sub.Id, snapshot, sku.OriginalPrice, line), ct);
                }
                responseSubs.Add(new(sub.Id, sub.SubOrderCode, sub.ShopId, sub.SubTotal, sub.Status.ToString()));
                eventSubs.Add(new(sub.Id, sub.ShopId, shop.Items.Sum(i => i.Quantity), sub.SubTotal));
            }
            var payment = Payment.Create(parent.Id, method, price.GrandTotal, now);
            await payments.AddAsync(payment, ct);
            carts.RemoveItems(selected);
            foreach (var voucher in draft.Vouchers.OrderBy(v => v.Id))
            {
                if (!await discounts.TryConsumeAsync(voucher.Id, buyer, clock.UtcNow, ct))
                    return CheckoutErrors.InvalidVoucher(voucher.Code, "Đã hết lượt dùng hoặc hết hiệu lực.");
                var amount = price.VoucherEvaluations.Single(v => v.VoucherCode == voucher.Code).DiscountAmount;
                await discounts.AddUsageAsync(DiscountUsage.Create(voucher.Id, buyer, parent.Id, amount, now), ct);
            }
            var response = new CheckoutResponse(parent.Id, parent.OrderCode, price.GrandTotal, method.ToString(),
                payment.Id, payment.Status.ToString(), responseSubs, payment.ExpiredAt);
            await outbox.AddAsync(new OutboxMessage("OrderPlacedEvent", JsonSerializer.Serialize(new OrderPlacedEvent(
                parent.Id, parent.OrderCode, buyer, eventSubs, method.ToString(), price.GrandTotal, key, response), Json)), ct);
            // T27 reserves Flash Sale separately; this checkout creates Online orders only.
            // T30/T32/T43/T47/T49 handle gateway, settlement, notifications, sale deduction and expiry.
            return response;
        }, false, ct);
    }
}
