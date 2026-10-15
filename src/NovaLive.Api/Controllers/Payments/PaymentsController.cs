using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NovaLive.Api.Controllers.Common;
using NovaLive.Api.Extensions;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Payments;
using NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;
using NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;

namespace NovaLive.Api.Controllers.Payments;

/// <summary>
/// API Controller quản lý cổng thanh toán trực tuyến (MoMo API v2, VietQR NAPAS 247) và tiếp nhận IPN/Webhook.
/// </summary>
[Route("api/v1/payments")]
public sealed class PaymentsController(
    ISender sender,
    IMomoPaymentAdapter momoAdapter,
    IVietQrPaymentAdapter vietQrAdapter,
    ICurrentUser currentUser,
    IParentOrderRepository parentOrderRepository,
    IPaymentRepository paymentRepository,
    ILogger<PaymentsController> logger) : ApiControllerBase(sender)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// IPN Webhook từ cổng MoMo sau khi khách hàng hoàn tất giao dịch.
    /// </summary>
    [HttpPost("webhook/momo")]
    [HttpPost("/payments/webhook/momo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MomoIpnResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MomoIpnResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MomoIpn(
        [FromBody] MomoIpnRequest request,
        CancellationToken ct)
    {
        logger.LogInformation(
            "Tiếp nhận MoMo IPN: OrderId={OrderId}, TransId={TransId}, Amount={Amount}, ResultCode={ResultCode}",
            request.OrderId, request.TransId, request.Amount, request.ResultCode);

        var result = await Sender.Send(new HandleMomoIpnCommand(request), ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Xử lý MoMo IPN thất bại: Code={Code}, Message={Description}",
                result.Error.Code, result.Error.Description);

            return BadRequest(new MomoIpnResponse(99, result.Error.Description));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Webhook tiếp nhận biến động số dư chuyển khoản VietQR NAPAS 247 (SePAY / Casso / VietQR).
    /// Hỗ trợ cả suffix /vqr (theo URL callback VietQR).
    /// </summary>
    [HttpPost("webhook/vietqr")]
    [HttpPost("/payments/webhook/vietqr")]
    [HttpPost("/vqr")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VietQrWebhookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(VietQrWebhookResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VietQrWebhook(
        [FromBody] VietQrWebhookRequest request,
        CancellationToken ct)
    {
        var signature = Request.Headers["X-Webhook-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
        {
            signature = Request.Headers["X-Signature"].ToString();
        }

        string rawBody = string.Empty;
        if (Request.Body != null && Request.Body.CanSeek)
        {
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            rawBody = await reader.ReadToEndAsync(ct);
            Request.Body.Position = 0;
        }

        if (string.IsNullOrWhiteSpace(rawBody) && request is not null)
        {
            rawBody = JsonSerializer.Serialize(request, JsonOptions);
        }

        logger.LogInformation(
            "Tiếp nhận VietQR Webhook: TransId={TransId}, Amount={Amount}, Content={Content}",
            request?.TransactionId, request?.Amount, request?.Content);

        var result = await Sender.Send(new HandleVietQrWebhookCommand(request, rawBody, signature), ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Xử lý VietQR Webhook thất bại: Code={Code}, Message={Description}",
                result.Error.Code, result.Error.Description);

            return BadRequest(new VietQrWebhookResponse(false, result.Error.Description));
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Endpoint ping kiểm tra kết nối liveness cho cổng VietQR Webhook (hỗ trợ /vqr).
    /// </summary>
    [HttpGet("webhook/vietqr")]
    [HttpGet("/payments/webhook/vietqr")]
    [HttpGet("/vqr")]
    [AllowAnonymous]
    public IActionResult VietQrPing()
    {
        return Ok(new
        {
            status = "active",
            gateway = "VietQR",
            serverTime = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Tạo liên kết thanh toán MoMo (QR Code / Deeplink App) cho đơn hàng của người dùng hiện tại.
    /// Số tiền được tải trực tiếp từ Payment trên hệ thống để ngăn ngừa giả mạo giá trị thanh toán.
    /// </summary>
    [HttpPost("momo/create")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MomoCreatePaymentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MomoCreatePaymentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<MomoCreatePaymentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMomoPayment(
        [FromBody] CreateMomoPaymentUrlRequest request,
        CancellationToken ct)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        ParentOrder? order = null;
        if (Guid.TryParse(request.OrderId, out var orderId))
        {
            order = await parentOrderRepository.FindAsync(orderId, currentUser.UserId.Value, ct);
        }

        if (order is null)
        {
            order = await parentOrderRepository.FindByOrderCodeAsync(request.OrderId, currentUser.UserId.Value, ct);
        }

        if (order is null)
        {
            return Result.Failure<MomoCreatePaymentResponse>(PaymentErrors.OrderNotFound).ToActionResult();
        }

        if (order.PaymentStatus != ParentOrderPaymentStatus.Pending)
        {
            return Result.Failure<MomoCreatePaymentResponse>(
                Error.Validation("Payment.OrderNotPending", "Đơn hàng không ở trạng thái chờ thanh toán.")).ToActionResult();
        }

        var payment = await paymentRepository.GetByParentOrderIdAsync(order.Id, ct);
        if (payment is null)
        {
            return Result.Failure<MomoCreatePaymentResponse>(
                Error.NotFound("Payment.NotFound", "Không tìm thấy thông tin thanh toán của đơn hàng.")).ToActionResult();
        }

        var momoAmount = (long)payment.Amount;
        var response = await momoAdapter.CreatePaymentUrlAsync(
            order.OrderCode,
            momoAmount,
            request.OrderInfo ?? string.Empty,
            request.ExtraData,
            ct);

        return Ok(ApiResponse<MomoCreatePaymentResponse>.Ok(response, "Khởi tạo thanh toán MoMo thành công."));
    }

    /// <summary>
    /// Sinh chuỗi mã VietQR chuẩn NAPAS 247 và QuickLink ảnh QR theo cú pháp NOVA {orderCode}.
    /// Số tiền thanh toán được tải trực tiếp từ Payment trên hệ thống của người dùng hiện tại.
    /// </summary>
    [HttpGet("vietqr/generate/{orderCode}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<VietQrResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<VietQrResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<VietQrResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateVietQr(
        [FromRoute] string orderCode,
        [FromQuery] string? description = null,
        CancellationToken ct = default)
    {
        if (!currentUser.UserId.HasValue)
        {
            return Unauthorized();
        }

        ParentOrder? order = null;
        if (Guid.TryParse(orderCode, out var orderId))
        {
            order = await parentOrderRepository.FindAsync(orderId, currentUser.UserId.Value, ct);
        }

        if (order is null)
        {
            order = await parentOrderRepository.FindByOrderCodeAsync(orderCode, currentUser.UserId.Value, ct);
        }

        if (order is null)
        {
            return Result.Failure<VietQrResponse>(PaymentErrors.OrderNotFound).ToActionResult();
        }

        if (order.PaymentStatus != ParentOrderPaymentStatus.Pending)
        {
            return Result.Failure<VietQrResponse>(
                Error.Validation("Payment.OrderNotPending", "Đơn hàng không ở trạng thái chờ thanh toán.")).ToActionResult();
        }

        var payment = await paymentRepository.GetByParentOrderIdAsync(order.Id, ct);
        if (payment is null)
        {
            return Result.Failure<VietQrResponse>(
                Error.NotFound("Payment.NotFound", "Không tìm thấy thông tin thanh toán của đơn hàng.")).ToActionResult();
        }

        var response = vietQrAdapter.GenerateDynamicQr(order.OrderCode, payment.Amount, description);
        return Ok(ApiResponse<VietQrResponse>.Ok(response, "Tạo mã VietQR thành công."));
    }
}
