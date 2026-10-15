using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Shops;
using NovaLive.Domain.System;

namespace NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;

public sealed class HandleMomoIpnCommandHandler(
    IMomoPaymentAdapter momoAdapter,
    IPaymentRepository paymentRepository,
    IParentOrderRepository parentOrderRepository,
    ISubOrderRepository subOrderRepository,
    IShopWalletRepository shopWalletRepository,
    IOutboxRepository outboxRepository,
    IDateTimeProvider clock,
    ILogger<HandleMomoIpnCommandHandler> logger)
    : ICommandHandler<HandleMomoIpnCommand, MomoIpnResponse>
{
    public async Task<Result<MomoIpnResponse>> Handle(
        HandleMomoIpnCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        logger.LogInformation(
            "Nhận IPN từ MoMo: PartnerCode={PartnerCode}, OrderId={OrderId}, TransId={TransId}, Amount={Amount}, ResultCode={ResultCode}",
            request.PartnerCode, request.OrderId, request.TransId, request.Amount, request.ResultCode);

        // 1. Xác thực HMAC-SHA256 signature
        if (!momoAdapter.VerifySignature(request))
        {
            logger.LogWarning("Chữ ký HMAC MoMo IPN không hợp lệ cho OrderId: {OrderId}", request.OrderId);
            return PaymentErrors.InvalidSignature;
        }

        var now = clock.UtcNow;
        var transIdStr = request.TransId.ToString();

        // 2. Idempotency Check: Tra cứu Payments WHERE transaction_ref = momoTransId
        var existingByTrans = await paymentRepository.GetByTransactionRefAsync(transIdStr, cancellationToken);
        if (existingByTrans is not null && existingByTrans.Status == PaymentStatus.Success)
        {
            logger.LogInformation("Giao dịch MoMo TransId={TransId} đã được xử lý thành công trước đó (Idempotent).", request.TransId);
            return new MomoIpnResponse(0, "Transaction already processed");
        }

        // 3. Tra cứu Payment & ParentOrder theo OrderId (hỗ trợ cả Guid ParentOrderId hoặc OrderCode)
        Payment? payment = null;
        NovaLive.Domain.Orders.ParentOrder? parentOrder = null;

        if (Guid.TryParse(request.OrderId, out var parentOrderId))
        {
            payment = await paymentRepository.GetForUpdateAsync(parentOrderId, cancellationToken);
            parentOrder = await parentOrderRepository.GetForUpdateAsync(parentOrderId, cancellationToken);
        }

        if (payment is null)
        {
            parentOrder = await parentOrderRepository.GetByOrderCodeForUpdateAsync(request.OrderId, cancellationToken);
            if (parentOrder is not null)
            {
                payment = await paymentRepository.GetForUpdateAsync(parentOrder.Id, cancellationToken);
            }
        }

        if (payment is null || parentOrder is null)
        {
            logger.LogWarning("Không tìm thấy Payment hoặc ParentOrder với mã: {OrderId}", request.OrderId);
            return PaymentErrors.PaymentNotFound;
        }

        // Kiểm tra nếu đơn hàng đã được đánh dấu thanh toán trước đó
        if (payment.Status == PaymentStatus.Success)
        {
            logger.LogInformation("Đơn hàng {OrderCode} đã ở trạng thái thanh toán thành công (Idempotent).", parentOrder.OrderCode);
            return new MomoIpnResponse(0, "Order already paid");
        }

        // Kiểm tra số tiền thanh toán từ MoMo khớp chính xác với số tiền của Payment
        var expectedAmount = (long)Math.Round(payment.Amount, MidpointRounding.AwayFromZero);
        if (request.Amount != expectedAmount)
        {
            logger.LogWarning("Số tiền MoMo IPN ({ReceivedAmount}) không khớp với số tiền đơn hàng ({RequiredAmount}) cho OrderId: {OrderId}",
                request.Amount, expectedAmount, request.OrderId);
            return PaymentErrors.InvalidAmount;
        }

        var rawJson = JsonSerializer.Serialize(request);

        // 4. Nếu ResultCode == 0: Thanh toán MoMo thành công
        if (request.ResultCode == 0)
        {
            // Cập nhật Payments
            var paymentResult = payment.MarkSuccess(transIdStr, now, rawJson);
            if (paymentResult.IsFailure)
            {
                logger.LogWarning("Cập nhật Payment thất bại: {Error}", paymentResult.Error.Description);
                return paymentResult.Error;
            }

            // Cập nhật ParentOrders
            var orderPaidResult = parentOrder.MarkPaid(now);
            if (orderPaidResult.IsFailure)
            {
                logger.LogWarning("Cập nhật ParentOrder thất bại: {Error}", orderPaidResult.Error.Description);
                return orderPaidResult.Error;
            }

            // Lấy danh sách SubOrders để chuyển sang Confirmed và kết chuyển doanh thu ví Shop
            var subOrders = await subOrderRepository.GetByParentIdsAsync([parentOrder.Id], tracked: true, cancellationToken);

            foreach (var subOrder in subOrders)
            {
                // Chuyển trạng thái SubOrder sang Confirmed
                subOrder.ConfirmPayment(now);

                // Ghi nhận doanh thu trực tiếp vào ShopWallets.balance của Shop (khoá dòng bảo vệ race condition)
                var wallet = await shopWalletRepository.GetByShopIdForUpdateAsync(subOrder.ShopId, cancellationToken);

                if (wallet is null)
                {
                    wallet = new ShopWallet(subOrder.ShopId);
                    await shopWalletRepository.AddAsync(wallet, cancellationToken);
                }

                var balanceBefore = wallet.Balance;
                var revenueAmount = subOrder.SubTotal;
                wallet.Credit(revenueAmount, now);

                // Ghi sổ cái ShopWalletTransactions
                var transaction = ShopWalletTransaction.Create(
                    walletId: wallet.Id,
                    type: WalletTxType.OrderRevenue,
                    amount: revenueAmount,
                    balanceBefore: balanceBefore,
                    balanceAfter: wallet.Balance,
                    refType: "SubOrder",
                    refId: subOrder.Id,
                    description: $"Doanh thu đơn hàng {subOrder.SubOrderCode}",
                    now: now);

                await shopWalletRepository.AddTransactionAsync(transaction, cancellationToken);
            }

            // Ghi OutboxMessage(PaymentSuccessEvent)
            var paymentEvent = new PaymentSuccessEvent(
                payment.Id,
                parentOrder.Id,
                parentOrder.OrderCode,
                payment.Amount,
                PaymentMethod.MoMo.ToString(),
                payment.TransactionRef,
                now);

            await outboxRepository.AddAsync(
                new OutboxMessage("PaymentSuccessEvent", JsonSerializer.Serialize(paymentEvent)),
                cancellationToken);

            logger.LogInformation(
                "Xử lý thành công IPN MoMo cho đơn hàng {OrderCode}. Doanh thu đã được cộng vào ví cho {ShopCount} shops.",
                parentOrder.OrderCode, subOrders.Count);

            return new MomoIpnResponse(0, "Payment processed successfully");
        }
        else
        {
            // MoMo báo lỗi hoặc giao dịch thất bại / người dùng hủy
            logger.LogWarning(
                "Giao dịch MoMo thất bại với ResultCode={ResultCode}, Message={Message}",
                request.ResultCode, request.Message);

            payment.MarkFailed(rawJson, now);
            parentOrder.CancelPayment(now);
            return new MomoIpnResponse(0, "Payment failed recorded");
        }
    }
}
