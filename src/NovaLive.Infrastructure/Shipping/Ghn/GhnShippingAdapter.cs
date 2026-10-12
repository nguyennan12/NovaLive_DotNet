using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Shipping.Ghn;

public sealed class GhnShippingAdapter(
    HttpClient httpClient,
    IOptions<GhnOptions> options,
    ILogger<GhnShippingAdapter> logger) : IShippingProvider
{
    private readonly GhnOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string ProviderCode => "GHN";

    public async Task<Result<ShippingFeeQuote>> CalculateFeeAsync(
        ShippingFeeCalculationModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var feeRequest = new GhnFeeRequest
            {
                FromDistrictId = request.FromDistrictId,
                FromWardCode = request.FromWardCode,
                ToDistrictId = request.ToDistrictId,
                ToWardCode = request.ToWardCode,
                Weight = request.WeightGram,
                Length = request.LengthCm,
                Width = request.WidthCm,
                Height = request.HeightCm,
                ServiceTypeId = request.ServiceTypeId ?? _options.DefaultServiceTypeId,
                InsuranceValue = (int)request.InsuranceValue
            };

            var httpResponse = await httpClient.PostAsJsonAsync(
                "/shiip/public-api/v2/shipping-order/fee",
                feeRequest,
                cancellationToken);

            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GhnApiResponse<GhnFeeData>>(content, JsonOptions);

            if (result is null || result.Code != 200 || result.Data is null)
            {
                var errorMessage = result?.Message ?? $"GHN trả lỗi mã {httpResponse.StatusCode}";
                logger.LogWarning("GHN fee calculation failed: {Error}", errorMessage);
                return Result<ShippingFeeQuote>.Failure(new Error(ErrorType.Validation, "Ghn.FeeCalculationFailed", errorMessage));
            }

            var quote = new ShippingFeeQuote(
                ProviderCode: ProviderCode,
                ServiceCode: feeRequest.ServiceTypeId.ToString(),
                TotalFee: result.Data.Total);

            return Result<ShippingFeeQuote>.Success(quote);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception when calling GHN fee API");
            return Result<ShippingFeeQuote>.Failure(Error.Failure("Ghn.CommunicationError", ex.Message));
        }
    }

    public async Task<Result<ShippingOrderCreateResult>> CreateShipmentAsync(
        ShippingShipmentCreateModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var createRequest = new GhnCreateOrderRequest
            {
                PaymentTypeId = 1,
                RequiredNote = string.IsNullOrWhiteSpace(request.RequiredNote) ? "KHONGCHOXEMHANG" : request.RequiredNote,
                FromName = request.FromName,
                FromPhone = request.FromPhone,
                FromAddress = request.FromAddress,
                FromWardName = request.FromWardName,
                FromDistrictName = request.FromDistrictName,
                ToName = request.ToName,
                ToPhone = request.ToPhone,
                ToAddress = request.ToAddress,
                ToWardCode = request.ToWardCode,
                ToDistrictId = request.ToDistrictId,
                Weight = request.WeightGram,
                Length = request.LengthCm,
                Width = request.WidthCm,
                Height = request.HeightCm,
                ServiceTypeId = request.ServiceTypeId ?? _options.DefaultServiceTypeId,
                CodAmount = (int)request.CodAmount,
                ClientOrderCode = request.OrderCode,
                Items = request.Items.Select(i => new GhnItemRequest
                {
                    Name = i.Name,
                    Code = i.Code,
                    Quantity = i.Quantity,
                    Weight = i.WeightGram
                }).ToList()
            };

            var httpResponse = await httpClient.PostAsJsonAsync(
                "/shiip/public-api/v2/shipping-order/create",
                createRequest,
                cancellationToken);

            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GhnApiResponse<GhnCreateOrderData>>(content, JsonOptions);

            if (result is null || result.Code != 200 || result.Data is null)
            {
                var errorMessage = result?.Message ?? $"GHN tạo vận đơn lỗi mã {httpResponse.StatusCode}";
                logger.LogWarning("GHN create shipment failed: {Error}", errorMessage);
                return Result<ShippingOrderCreateResult>.Failure(new Error(ErrorType.Validation, "Ghn.CreateShipmentFailed", errorMessage));
            }

            var createResult = new ShippingOrderCreateResult(
                ProviderOrderCode: result.Data.OrderCode,
                TrackingCode: result.Data.OrderCode,
                TotalFee: result.Data.TotalFee,
                ExpectedDeliveryDate: result.Data.ExpectedDeliveryTime);

            return Result<ShippingOrderCreateResult>.Success(createResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception when calling GHN create order API");
            return Result<ShippingOrderCreateResult>.Failure(Error.Failure("Ghn.CommunicationError", ex.Message));
        }
    }

    public async Task<Result<byte[]>> GetShippingLabelAsync(
        string trackingCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var genTokenRequest = new GhnGenTokenRequest
            {
                OrderCodes = [trackingCode]
            };

            var httpResponse = await httpClient.PostAsJsonAsync(
                "/shiip/public-api/v2/a5/gen-token",
                genTokenRequest,
                cancellationToken);

            var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GhnApiResponse<GhnGenTokenData>>(content, JsonOptions);

            if (result is null || result.Code != 200 || result.Data is null || string.IsNullOrWhiteSpace(result.Data.Token))
            {
                var errorMessage = result?.Message ?? "Không thể lấy token in vận đơn GHN";
                return Result<byte[]>.Failure(new Error(ErrorType.Validation, "Ghn.PrintTokenFailed", errorMessage));
            }

            var downloadUrl = $"/a5/public-api/printA5?token={result.Data.Token}";
            var pdfResponse = await httpClient.GetAsync(downloadUrl, cancellationToken);

            if (!pdfResponse.IsSuccessStatusCode)
            {
                return Result<byte[]>.Failure(Error.Failure("Ghn.DownloadLabelFailed", "Không thể tải phiếu in PDF từ GHN."));
            }

            var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync(cancellationToken);
            return Result<byte[]>.Success(pdfBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception when downloading shipping label from GHN");
            return Result<byte[]>.Failure(Error.Failure("Ghn.CommunicationError", ex.Message));
        }
    }
}

