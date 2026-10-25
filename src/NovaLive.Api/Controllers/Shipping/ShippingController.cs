using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    IGhnChecksumValidator checksumValidator,
    IHttpClientFactory httpClientFactory,
    IOptions<GhnOptions> ghnOptions,
    ILogger<ShippingController> logger) : ApiControllerBase(sender)
{
    private readonly GhnOptions _ghn = ghnOptions.Value;

    [HttpGet("provinces")]
    [AllowAnonymous]
    public Task<IActionResult> GetProvinces(CancellationToken ct) =>
        GetMasterDataAsync("master-data/province", ct);

    [HttpGet("districts")]
    [AllowAnonymous]
    public Task<IActionResult> GetDistricts(
        [FromQuery(Name = "province_id")] int provinceId,
        CancellationToken ct) => provinceId > 0
        ? GetMasterDataAsync($"master-data/district?province_id={provinceId}", ct)
        : Task.FromResult<IActionResult>(BadRequest(new { message = "province_id phải lớn hơn 0." }));

    [HttpGet("wards")]
    [AllowAnonymous]
    public Task<IActionResult> GetWards(
        [FromQuery(Name = "district_id")] int districtId,
        CancellationToken ct) => districtId > 0
        ? GetMasterDataAsync($"master-data/ward?district_id={districtId}", ct)
        : Task.FromResult<IActionResult>(BadRequest(new { message = "district_id phải lớn hơn 0." }));

    private async Task<IActionResult> GetMasterDataAsync(string path, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_ghn.ApiToken))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "GHN chưa được cấu hình API token." });

        if (!Uri.TryCreate(_ghn.BaseUrl, UriKind.Absolute, out var baseUri))
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { message = "GHN chưa được cấu hình URL hợp lệ." });

        try
        {
            var url = new Uri(baseUri, $"shiip/public-api/{path}");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Token", _ghn.ApiToken);
            if (_ghn.ShopId > 0)
                request.Headers.TryAddWithoutValidation("ShopId", _ghn.ShopId.ToString());

            using var response = await httpClientFactory.CreateClient().SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("GHN master-data request failed with status {StatusCode}.", response.StatusCode);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "GHN không trả được dữ liệu địa chỉ." });
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = document.RootElement;
            if (!root.TryGetProperty("code", out var code)
                || code.ValueKind != JsonValueKind.Number
                || code.GetInt32() != 200
                || !root.TryGetProperty("data", out var data)
                || data.ValueKind != JsonValueKind.Array)
            {
                logger.LogWarning("GHN master-data request returned an invalid response for {Path}.", path.Split('?')[0]);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "GHN trả dữ liệu địa chỉ không hợp lệ." });
            }

            return Ok(new { data = data.Clone() });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Could not fetch GHN master data for {Path}.", path.Split('?')[0]);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { message = "Không thể kết nối dịch vụ địa chỉ GHN." });
        }
    }

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
            ShippingProvider.GHN, trackingCode, status.Value, rawJson, request.UpdatedAt,
            IsVerifiedWebhook: true), ct);
        return Respond(result, message: "Webhook processed successfully.");
    }
}
