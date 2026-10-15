using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;
using NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Api.Controllers.Payments;

/// <summary>
/// API Controller quản lý cổng thanh toán trực tuyến (MoMo API v2, VietQR NAPAS 247) và tiếp nhận IPN/Webhook.
/// </summary>
[Route("api/v1/payments")]
public sealed class PaymentsController(
    ISender sender,
    IMomoPaymentAdapter momoAdapter,
    IVietQrPaymentAdapter vietQrAdapter,
    ILogger<PaymentsController> logger) : ApiControllerBase(sender)
{
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

        logger.LogInformation(
            "Tiếp nhận VietQR Webhook: TransId={TransId}, Amount={Amount}, Content={Content}",
            request.TransactionId, request.Amount, request.Content);

        var result = await Sender.Send(new HandleVietQrWebhookCommand(request, signature), ct);

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
    /// Tạo liên kết thanh toán MoMo (QR Code / Deeplink App) cho đơn hàng.
    /// </summary>
    [HttpPost("momo/create")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MomoCreatePaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateMomoPayment(
        [FromBody] CreateMomoPaymentUrlRequest request,
        CancellationToken ct)
    {
        var response = await momoAdapter.CreatePaymentUrlAsync(
            request.OrderId,
            request.Amount,
            request.OrderInfo,
            request.ExtraData,
            ct);

        return Ok(ApiResponse<MomoCreatePaymentResponse>.Ok(response, "Khởi tạo thanh toán MoMo thành công."));
    }

    /// <summary>
    /// Sinh chuỗi mã VietQR chuẩn NAPAS 247 và QuickLink ảnh QR theo cú pháp NOVA {orderCode}.
    /// </summary>
    [HttpGet("vietqr/generate/{orderCode}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<VietQrResponse>), StatusCodes.Status200OK)]
    public IActionResult GenerateVietQr(
        [FromRoute] string orderCode,
        [FromQuery] decimal amount,
        [FromQuery] string? description = null)
    {
        var response = vietQrAdapter.GenerateDynamicQr(orderCode, amount, description);
        return Ok(ApiResponse<VietQrResponse>.Ok(response, "Tạo mã VietQR thành công."));
    }
}
