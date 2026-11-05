using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Payments.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandHandler(
    ICurrentUser user, IParentOrderRepository orders, IPaymentRepository payments,
    IMomoPaymentAdapter momo, IVietQrPaymentAdapter vietQr, IDateTimeProvider clock)
    : ICommandHandler<InitiatePaymentCommand, PaymentInitResponse>
{
    public async Task<Result<PaymentInitResponse>> Handle(InitiatePaymentCommand command, CancellationToken ct)
    {
        if (user.UserId is not { } buyer || buyer == Guid.Empty) return PaymentErrors.Unauthorized;
        var request = command.Request;
        if (!Enum.GetNames<PaymentMethod>().Contains(request.PaymentMethod, StringComparer.OrdinalIgnoreCase))
            return PaymentErrors.InvalidMethod;
        var method = Enum.Parse<PaymentMethod>(request.PaymentMethod, true);
        var order = await orders.FindAsync(request.ParentOrderId, buyer, ct);
        if (order is null) return PaymentErrors.OrderNotFound;
        var payment = await payments.GetByParentOrderIdAsync(order.Id, ct);
        if (payment is null) return PaymentErrors.PaymentNotFound;
        var validation = PaymentInitiationRules.Validate(order, payment, method, clock.UtcNow);
        if (validation.IsFailure) return validation.Error;

        string? url = null, payload = null, image = null;
        VietQrResponse? qrDetails = null;
        try
        {
            if (method == PaymentMethod.MoMo)
            {
                // Use the order code, which the IPN handler resolves; never the Payment ID.
                if (payment.Amount != decimal.Truncate(payment.Amount) || payment.Amount > long.MaxValue)
                    return PaymentErrors.InvalidAmount;
                var response = await momo.CreatePaymentUrlAsync(order.OrderCode, (long)payment.Amount,
                    $"Thanh toan {order.OrderCode}", null, ct);
                if (response.ResultCode != 0 || string.IsNullOrWhiteSpace(response.PayUrl))
                    return PaymentErrors.GatewayFailed("Không thể tạo liên kết thanh toán MoMo.");
                url = response.PayUrl;
            }
            else if (method == PaymentMethod.VietQR)
            {
                var response = await vietQr.GenerateDynamicQrAsync(order.OrderCode, payment.Amount, cancellationToken: ct);
                qrDetails = response;
                payload = response.QrCode;
                image = response.QrImageBase64;
            }
        }
        catch (HttpRequestException)
        {
            return PaymentErrors.GatewayFailed("Không kết nối được cổng thanh toán. Vui lòng thử lại.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return PaymentErrors.GatewayFailed("Cổng thanh toán phản hồi quá thời gian chờ.");
        }
        catch (InvalidOperationException exception)
        {
            return PaymentErrors.GatewayFailed(exception.Message);
        }
        // A slow gateway call must not hand the buyer an already expired payment.
        if (payment.IsDueForExpiry(clock.UtcNow, TimeSpan.FromMinutes(15))) return PaymentErrors.PaymentExpired;
        return new PaymentInitResponse(payment.Id, order.Id, method.ToString(), payment.Amount,
            payment.Status.ToString(), url, payload, image,
            method == PaymentMethod.COD ? null : payment.ExpiredAt ?? payment.CreatedAt.AddMinutes(15), qrDetails);
    }
}
