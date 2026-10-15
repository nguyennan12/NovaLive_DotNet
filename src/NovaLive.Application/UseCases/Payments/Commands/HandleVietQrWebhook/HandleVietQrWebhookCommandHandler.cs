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

namespace NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;

/// <summary>
/// Command Handler xử lý webhook VietQR: trích xuất orderCode, kiểm tra số tiền khớp, cập nhật thanh toán và cộng ví Shop.
/// </summary>
public sealed class HandleVietQrWebhookCommandHandler(
    IVietQrPaymentAdapter vietQrAdapter,
    IPaymentRepository paymentRepository,
    IParentOrderRepository parentOrderRepository,
    ISubOrderRepository subOrderRepository,
    IShopWalletRepository shopWalletRepository,
    IOutboxRepository outboxRepository,
    IDateTimeProvider clock,
    ILogger<HandleVietQrWebhookCommandHandler> logger)
    : ICommandHandler<HandleVietQrWebhookCommand, VietQrWebhookResponse>
{
    public async Task<Result<VietQrWebhookResponse>> Handle(
        HandleVietQrWebhookCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var transId = request.ResolvedTransactionId;
        var content = request.ResolvedContent;

        logger.LogInformation(
            "Nhận Webhook VietQR: TransId={TransId}, Amount={Amount}, Content={Content}, Bank={Bank}",
            transId, request.Amount, content, request.BankBrand);

        // 1. Phân tích nội dung chuyển khoản tìm orderCode (theo cú pháp NOVA {order_code} hoặc tương đương)
        var orderCode = vietQrAdapter.ExtractOrderCode(content);
        if (string.IsNullOrWhiteSpace(orderCode))
        {
            logger.LogWarning("Không thể trích xuất orderCode từ nội dung chuyển khoản: '{Content}'", content);
            return PaymentErrors.CannotParseOrderCode;
        }

        // 2. Tra cứu đơn hàng ParentOrders theo orderCode
        var parentOrder = await parentOrderRepository.GetByOrderCodeForUpdateAsync(orderCode, cancellationToken);
        if (parentOrder is null)
        {
            logger.LogWarning("Không tìm thấy đơn hàng với mã: {OrderCode}", orderCode);
            return PaymentErrors.OrderNotFound;
        }

        // 3. Tra cứu Payments của đơn hàng
        var payment = await paymentRepository.GetForUpdateAsync(parentOrder.Id, cancellationToken);
        if (payment is null)
        {
            logger.LogWarning("Không tìm thấy Payment cho đơn hàng: {OrderCode}", orderCode);
            return PaymentErrors.PaymentNotFound;
        }

        // 4. Idempotency Check: nếu giao dịch này hoặc đơn hàng đã thanh toán thành công trước đó
        if (payment.Status == PaymentStatus.Success)
        {
            logger.LogInformation(
                "Đơn hàng {OrderCode} đã được ghi nhận thanh toán thành công trước đó (Idempotent).",
                orderCode);
            return new VietQrWebhookResponse(true, "Transaction already processed", orderCode);
        }

        // 5. Kiểm tra số tiền chuyển khớp chính xác: amount >= Payments.amount
        if (request.Amount < payment.Amount)
        {
            logger.LogWarning(
                "Số tiền chuyển khoản ({ReceivedAmount}) nhỏ hơn số tiền đơn hàng ({RequiredAmount}) cho đơn {OrderCode}",
                request.Amount, payment.Amount, orderCode);
            return PaymentErrors.InvalidAmount;
        }

        var now = clock.UtcNow;
        var rawJson = JsonSerializer.Serialize(request);

        // 6. Cập nhật Payments.status = Success, transaction_ref = bankRefId, paid_at = NOW()
        payment.MarkSuccess(transId, now, rawJson);

        // 7. Cập nhật ParentOrders.payment_status = Paid
        parentOrder.MarkPaid(now);

        // 8. Chuyển trạng thái các SubOrders sang Confirmed và cộng doanh thu vào ví Shop
        var subOrders = await subOrderRepository.GetByParentIdsAsync([parentOrder.Id], tracked: true, cancellationToken);

        foreach (var subOrder in subOrders)
        {
            subOrder.ConfirmPayment(now);

            // Ghi nhận doanh thu trực tiếp vào ShopWallets.balance của Shop
            var wallet = await shopWalletRepository.GetByShopIdAsync(subOrder.ShopId, cancellationToken);

            if (wallet is null)
            {
                wallet = new ShopWallet(subOrder.ShopId);
                await shopWalletRepository.AddAsync(wallet, cancellationToken);
            }

            var balanceBefore = wallet.Balance;
            var revenueAmount = subOrder.SubTotal;
            wallet.Credit(revenueAmount, now);

            // Ghi log sổ cái ShopWalletTransactions (OrderRevenue)
            var transaction = ShopWalletTransaction.Create(
                walletId: wallet.Id,
                type: WalletTxType.OrderRevenue,
                amount: revenueAmount,
                balanceBefore: balanceBefore,
                balanceAfter: wallet.Balance,
                refType: "SubOrder",
                refId: subOrder.Id,
                description: $"Doanh thu từ đơn hàng {subOrder.SubOrderCode}",
                now: now);

            await shopWalletRepository.AddTransactionAsync(transaction, cancellationToken);
        }

        // 9. Ghi OutboxMessage(PaymentSuccessEvent)
        var paymentEvent = new PaymentSuccessEvent(
            payment.Id,
            parentOrder.Id,
            parentOrder.OrderCode,
            payment.Amount,
            PaymentMethod.VietQR.ToString(),
            payment.TransactionRef,
            now);

        await outboxRepository.AddAsync(
            new OutboxMessage("PaymentSuccessEvent", JsonSerializer.Serialize(paymentEvent)),
            cancellationToken);

        logger.LogInformation(
            "Xử lý thành công VietQR Webhook cho đơn hàng {OrderCode} (TransId: {TransId}). Đã kết chuyển doanh thu cho {ShopCount} shops.",
            orderCode, transId, subOrders.Count);

        return new VietQrWebhookResponse(true, "Payment processed successfully", orderCode);
    }
}
