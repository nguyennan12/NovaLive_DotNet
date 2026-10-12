using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping.Commands.Webhook;

public sealed class HandleShippingWebhookCommandHandler(
    IAppDbContext dbContext,
    ILogger<HandleShippingWebhookCommandHandler> logger)
    : ICommandHandler<HandleShippingWebhookCommand>
{
    public async Task<Result> Handle(HandleShippingWebhookCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;

        // 1. Tìm kiếm đơn vận chuyển trong Database theo mã đơn GHN hoặc TrackingCode
        var orderCode = req.OrderCode.Trim();
        var trackingCode = req.TrackingCode?.Trim();

        var shippingOrder = await dbContext.ShippingOrders
            .FirstOrDefaultAsync(s =>
                s.TrackingCode == orderCode ||
                s.ProviderOrderId == orderCode ||
                (trackingCode != null && s.TrackingCode == trackingCode),
                cancellationToken);

        if (shippingOrder is null)
        {
            logger.LogWarning("Không tìm thấy đơn vận chuyển tương ứng với mã GHN: {OrderCode}", req.OrderCode);
            // Trả về Success để GHN không retry liên tục các đơn không thuộc hệ thống hoặc đơn test
            return Result.Success();
        }

        // 2. Chuyển đổi mã trạng thái từ GHN sang ShippingStatus
        var mappedStatus = MapGhnStatus(req.Status);

        // 3. Cập nhật trạng thái và payload snapshot vào ShippingOrder
        shippingOrder.UpdateStatusFromWebhook(
            newStatus: mappedStatus,
            webhookPayload: command.RawPayload,
            eventTime: req.UpdatedAt);

        // 4. Đồng bộ trạng thái SubOrder tương ứng (FR-SHIP-006, FR-SHIP-007)
        var subOrder = await dbContext.SubOrders
            .FirstOrDefaultAsync(s => s.Id == shippingOrder.SubOrderId, cancellationToken);

        if (subOrder is not null)
        {
            if (mappedStatus == ShippingStatus.Delivered)
            {
                subOrder.MarkAsDelivered(req.UpdatedAt);
            }
            else if (mappedStatus == ShippingStatus.Delivering || mappedStatus == ShippingStatus.Picking)
            {
                subOrder.MarkAsShipping(req.UpdatedAt);
            }
        }

        logger.LogInformation(
            "Cập nhật thành công trạng thái đơn vận chuyển SubOrder={SubOrderId}, TrackingCode={TrackingCode} sang {Status}",
            shippingOrder.SubOrderId,
            shippingOrder.TrackingCode,
            mappedStatus);

        return Result.Success();
    }


    private static ShippingStatus MapGhnStatus(string ghnStatus)
    {
        var normalized = ghnStatus.Trim().ToLowerInvariant();

        return normalized switch
        {
            "ready_for_pick" => ShippingStatus.ReadyToPick,
            "picking" => ShippingStatus.Picking,
            "storing" => ShippingStatus.Picking,
            "delivering" => ShippingStatus.Delivering,
            "delivered" => ShippingStatus.Delivered,
            "delivery_fail" => ShippingStatus.Failed,
            "return" or "returning" or "returned" => ShippingStatus.Returned,
            "cancel" or "cancelled" => ShippingStatus.Cancelled,
            _ => ShippingStatus.Delivering
        };
    }
}
