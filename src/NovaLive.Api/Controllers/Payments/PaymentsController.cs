using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
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
using NovaLive.Application.UseCases.Payments.Commands.InitiatePayment;
using NovaLive.Application.UseCases.Payments.Queries.GetPaymentStatus;
using NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;
using NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;

namespace NovaLive.Api.Controllers.Payments;

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

    [HttpPost("initiate")]
    [HttpPost("/payments/initiate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest request, CancellationToken ct) =>
        (await Sender.Send(new InitiatePaymentCommand(request), ct)).ToActionResult();

    [HttpGet("{paymentId:guid}/status")]
    [HttpGet("/payments/{paymentId:guid}/status")]
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(Guid paymentId, CancellationToken ct) =>
        (await Sender.Send(new GetPaymentStatusQuery(paymentId), ct)).ToActionResult();

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
        if (request is null)
        {
            return BadRequest(new VietQrWebhookResponse(false, "Dữ liệu webhook không được để trống."));
        }

        var signature = Request.Headers["X-Webhook-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
        {
            signature = Request.Headers["X-Signature"].ToString();
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            signature = request.Signature;
        }

        string rawBody = string.Empty;
        if (Request.Body != null && Request.Body.CanSeek)
        {
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            rawBody = await reader.ReadToEndAsync(ct);
            Request.Body.Position = 0;
        }

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            rawBody = JsonSerializer.Serialize(request, JsonOptions);
        }

        logger.LogInformation(
            "Tiếp nhận VietQR Webhook: TransId={TransId}, Amount={Amount}, Content={Content}",
            request.TransactionId, request.Amount, request.Content);

        var result = await Sender.Send(new HandleVietQrWebhookCommand(request, rawBody, signature), ct);

        if (result.IsFailure)
        {
            logger.LogWarning("Xử lý VietQR Webhook thất bại: Code={Code}, Message={Description}",
                result.Error.Code, result.Error.Description);

            return BadRequest(new VietQrWebhookResponse(false, result.Error.Description));
        }

        return Ok(result.Value);
    }

    [HttpPost("/vqr/api/token_generate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VietQrTokenResponse), StatusCodes.Status200OK)]
    public IActionResult VietQrCallbackToken()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var authorization)
            || !string.Equals(authorization.Scheme, "Basic", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(authorization.Parameter))
            return Unauthorized(new { status = "FAILED", message = "Authorization Basic không hợp lệ." });

        string decoded;
        try { decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization.Parameter)); }
        catch (FormatException) { return Unauthorized(new { status = "FAILED", message = "Authorization Basic không hợp lệ." }); }
        var separator = decoded.IndexOf(':');
        if (separator <= 0) return Unauthorized(new { status = "FAILED", message = "Authorization Basic không hợp lệ." });
        var username = decoded[..separator];
        var password = decoded[(separator + 1)..];
        return vietQrAdapter.TryIssueCallbackToken(username, password, out var token)
            ? Ok(token)
            : Unauthorized(new { status = "FAILED", message = "Thông tin kết nối VietQR không hợp lệ." });
    }

    [HttpPost("/bank/api/transaction-sync")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VietQrTransactionSyncResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> VietQrTransactionSync([FromBody] VietQrWebhookRequest request, CancellationToken ct)
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var authorization)
            || !string.Equals(authorization.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(authorization.Parameter)
            || !vietQrAdapter.VerifyCallbackToken(authorization.Parameter))
            return Unauthorized(new VietQrTransactionSyncResponse(true, "INVALID_TOKEN", "Bearer token không hợp lệ hoặc đã hết hạn.", null));

        if (string.IsNullOrWhiteSpace(request.BankAccount)
            || request.Amount <= 0
            || request.Amount != decimal.Truncate(request.Amount)
            || request.Amount > long.MaxValue
            || string.IsNullOrWhiteSpace(request.TransType)
            || string.IsNullOrWhiteSpace(request.Content)
            || string.IsNullOrWhiteSpace(request.TransactionId)
            || request.TransactionTime is null or <= 0
            || string.IsNullOrWhiteSpace(request.ReferenceNumber)
            || string.IsNullOrWhiteSpace(request.OrderId))
            return BadRequest(new VietQrTransactionSyncResponse(
                true, "INVALID_REQUEST", "Thiếu trường bắt buộc theo chuẩn VietQR Transaction Sync.", null));

        var result = await Sender.Send(new HandleVietQrWebhookCommand(
            request, JsonSerializer.Serialize(request, JsonOptions), IsAuthenticatedCallback: true), ct);
        if (result.IsFailure)
            return BadRequest(new VietQrTransactionSyncResponse(true, result.Error.Code, result.Error.Description, null));
        return Ok(new VietQrTransactionSyncResponse(false, null, "Transaction processed successfully",
            new VietQrTransactionSyncObject(request.ResolvedTransactionId!)));
    }

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

        var validation = PaymentInitiationRules.Validate(order, payment, PaymentMethod.MoMo, DateTimeOffset.UtcNow);
        if (validation.IsFailure) return Result.Failure<MomoCreatePaymentResponse>(validation.Error).ToActionResult();

        if (payment.Amount != decimal.Truncate(payment.Amount) || payment.Amount > long.MaxValue)
            return Result.Failure<MomoCreatePaymentResponse>(PaymentErrors.InvalidAmount).ToActionResult();
        var momoAmount = (long)payment.Amount;
        MomoCreatePaymentResponse response;
        try
        {
            response = await momoAdapter.CreatePaymentUrlAsync(
                order.OrderCode,
                momoAmount,
                request.OrderInfo ?? string.Empty,
                request.ExtraData,
                ct);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Không thể khởi tạo MoMo cho đơn {OrderCode}.", order.OrderCode);
            return Result.Failure<MomoCreatePaymentResponse>(
                Error.Validation("Payment.MomoUnavailable", exception.Message)).ToActionResult();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "MoMo gateway không phản hồi cho đơn {OrderCode}.", order.OrderCode);
            return Result.Failure<MomoCreatePaymentResponse>(
                Error.Failure("Payment.MomoGatewayUnavailable", "MoMo đang gặp sự cố kết nối. Hãy thử lại hoặc chọn phương thức thanh toán khác.")).ToActionResult();
        }
        catch (TaskCanceledException exception) when (!ct.IsCancellationRequested)
        {
            logger.LogError(exception, "MoMo gateway hết thời gian phản hồi cho đơn {OrderCode}.", order.OrderCode);
            return Result.Failure<MomoCreatePaymentResponse>(
                Error.Failure("Payment.MomoGatewayTimeout", "MoMo phản hồi quá lâu. Hãy thử lại hoặc chọn phương thức thanh toán khác.")).ToActionResult();
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Phản hồi MoMo không hợp lệ cho đơn {OrderCode}.", order.OrderCode);
            return Result.Failure<MomoCreatePaymentResponse>(
                Error.Failure("Payment.MomoInvalidResponse", "MoMo trả về dữ liệu không hợp lệ. Hãy thử lại sau.")).ToActionResult();
        }

        if (response.ResultCode != 0 || string.IsNullOrWhiteSpace(response.PayUrl))
            return Result.Failure<MomoCreatePaymentResponse>(PaymentErrors.GatewayFailed("Không thể tạo liên kết thanh toán MoMo.")).ToActionResult();
        return Ok(ApiResponse<MomoCreatePaymentResponse>.Ok(response, "Khởi tạo thanh toán MoMo thành công."));
    }

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

        var validation = PaymentInitiationRules.Validate(order, payment, PaymentMethod.VietQR, DateTimeOffset.UtcNow);
        if (validation.IsFailure) return Result.Failure<VietQrResponse>(validation.Error).ToActionResult();

        var response = await vietQrAdapter.GenerateDynamicQrAsync(order.OrderCode, payment.Amount, description, ct);
        return Ok(ApiResponse<VietQrResponse>.Ok(response, "Tạo mã VietQR thành công."));
    }

    [HttpPost("vietqr/test-callback/{orderCode}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<VietQrTestCallbackResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestVietQrCallback([FromRoute] string orderCode, CancellationToken ct)
    {
        if (currentUser.UserId is not { } buyerId) return Unauthorized();
        var order = await parentOrderRepository.FindByOrderCodeAsync(orderCode, buyerId, ct);
        if (order is null) return Result.Failure<VietQrTestCallbackResponse>(PaymentErrors.OrderNotFound).ToActionResult();
        var payment = await paymentRepository.GetByParentOrderIdAsync(order.Id, ct);
        if (payment is null) return Result.Failure<VietQrTestCallbackResponse>(PaymentErrors.PaymentNotFound).ToActionResult();
        var validation = PaymentInitiationRules.Validate(order, payment, PaymentMethod.VietQR, DateTimeOffset.UtcNow);
        if (validation.IsFailure) return Result.Failure<VietQrTestCallbackResponse>(validation.Error).ToActionResult();
        try
        {
            var qr = await vietQrAdapter.GenerateDynamicQrAsync(order.OrderCode, payment.Amount, cancellationToken: ct);
            var callback = await vietQrAdapter.TriggerTestCallbackAsync(qr, ct);
            return Ok(ApiResponse<VietQrTestCallbackResponse>.Ok(callback, "Đã yêu cầu VietQR Sandbox gửi Test Callback."));
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            logger.LogWarning(exception, "VietQR Test Callback thất bại cho đơn {OrderCode}.", order.OrderCode);
            return Result.Failure<VietQrTestCallbackResponse>(
                Error.Failure("Payment.VietQrTestCallbackFailed", exception.Message)).ToActionResult();
        }
    }
}
