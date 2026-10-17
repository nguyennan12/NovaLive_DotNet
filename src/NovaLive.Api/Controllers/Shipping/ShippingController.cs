using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Application.UseCases.Shipping.Queries.CalculateFee;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Infrastructure.Shipping.Ghn;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Controllers.Shipping;

[Route("api/v1/shipping")]
public sealed class ShippingController(
    ISender sender,
    IGhnChecksumValidator checksumValidator) : ApiControllerBase(sender)
{
    [HttpPost("calculate-fee")]
    [Authorize]
    public async Task<IActionResult> CalculateFee(
        [FromBody] CalculateShippingFeeRequest data,
        CancellationToken ct)
    {
        return Respond(await Sender.Send(new CalculateShippingFeeQuery(data), ct));
    }

    [HttpPost("webhook/ghn")]
    [AllowAnonymous]
    public async Task<IActionResult> GhnWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawJson = await reader.ReadToEndAsync(ct);
        var secretHeader = Request.Headers["X-Webhook-Secret"].ToString();
        var signatureHeader = Request.Headers["X-Ghn-Signature"].ToString();
        var tokenHeader = Request.Headers["Token"].ToString();

        if (!checksumValidator.ValidateWebhook(rawJson, secretHeader, signatureHeader, tokenHeader))
        {
            return Unauthorized(new { message = "Xác thực chữ ký hoặc Secret Webhook GHN thất bại." });
        }

        GhnWebhookRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<GhnWebhookRequest>(rawJson,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException) { return Respond(Result.Failure(ShippingErrors.InvalidWebhook)); }
        var status = ShippingStatusMapper.Map(ShippingProvider.GHN, request?.Status);
        if (request is null || status is null) return Respond(Result.Failure(ShippingErrors.InvalidWebhook));
        var trackingCode = string.IsNullOrWhiteSpace(request.TrackingCode) ? request.OrderCode : request.TrackingCode;
        var result = await Sender.Send(new HandleShippingWebhookCommand(
            ShippingProvider.GHN, trackingCode, status.Value, rawJson, request.UpdatedAt), ct);
        return Respond(result, message: "Webhook processed successfully.");
    }
}
