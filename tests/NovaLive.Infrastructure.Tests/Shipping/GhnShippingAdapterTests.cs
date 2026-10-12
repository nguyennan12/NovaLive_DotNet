using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Infrastructure.Shipping.Ghn;
using Xunit;

namespace NovaLive.Infrastructure.Tests.Shipping;

public class GhnShippingAdapterTests
{
    private readonly GhnOptions _options = new()
    {
        BaseUrl = "https://dev-online-gateway.ghn.vn",
        ApiToken = "ff432d05-3989-11f1-a973-aee5264794df",
        ShopId = 12345,
        DefaultServiceTypeId = 2
    };

    [Fact]
    public async Task CalculateFeeAsync_WhenGhnReturns200_ShouldReturnFeeQuote()
    {
        // Arrange
        var ghnResponse = new GhnApiResponse<GhnFeeData>
        {
            Code = 200,
            Message = "Success",
            Data = new GhnFeeData
            {
                Total = 25000,
                ServiceFee = 25000,
                InsuranceFee = 0
            }
        };

        var handler = new StubHandler((req, ct) =>
        {
            var res = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(ghnResponse))
            };
            return Task.FromResult(res);
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var adapter = new GhnShippingAdapter(client, Options.Create(_options), NullLogger<GhnShippingAdapter>.Instance);

        var request = new ShippingFeeCalculationModel(
            FromDistrictId: 1442,
            FromWardCode: "20108",
            ToDistrictId: 1530,
            ToWardCode: "20101",
            WeightGram: 500);

        // Act
        var result = await adapter.CalculateFeeAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalFee.Should().Be(25000);
        result.Value.ProviderCode.Should().Be("GHN");
    }

    [Fact]
    public async Task CalculateFeeAsync_WhenGhnReturnsError_ShouldReturnFailure()
    {
        // Arrange
        var ghnResponse = new GhnApiResponse<GhnFeeData>
        {
            Code = 400,
            Message = "from_district_id is invalid",
            Data = null
        };

        var handler = new StubHandler((req, ct) =>
        {
            var res = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(JsonSerializer.Serialize(ghnResponse))
            };
            return Task.FromResult(res);
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var adapter = new GhnShippingAdapter(client, Options.Create(_options), NullLogger<GhnShippingAdapter>.Instance);

        var request = new ShippingFeeCalculationModel(
            FromDistrictId: 0,
            FromWardCode: "",
            ToDistrictId: 1530,
            ToWardCode: "20101",
            WeightGram: 500);

        // Act
        var result = await adapter.CalculateFeeAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Ghn.FeeCalculationFailed");
        result.Error.Message.Should().Contain("from_district_id is invalid");
    }

    [Fact]
    public async Task CreateShipmentAsync_WhenValidRequest_ShouldReturnTrackingCode()
    {
        // Arrange
        var ghnResponse = new GhnApiResponse<GhnCreateOrderData>
        {
            Code = 200,
            Message = "Success",
            Data = new GhnCreateOrderData
            {
                OrderCode = "GHN12345678",
                TotalFee = 32000,
                ExpectedDeliveryTime = DateTime.UtcNow.AddDays(2)
            }
        };

        var handler = new StubHandler((req, ct) =>
        {
            var res = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(ghnResponse))
            };
            return Task.FromResult(res);
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var adapter = new GhnShippingAdapter(client, Options.Create(_options), NullLogger<GhnShippingAdapter>.Instance);

        var request = new ShippingShipmentCreateModel(
            OrderCode: "ORDER_999",
            FromName: "Kho Nova",
            FromPhone: "0900000000",
            FromAddress: "123 Kho Hàng",
            FromWardName: "Phường A",
            FromDistrictName: "Quận 1",
            FromDistrictId: 1442,
            FromWardCode: "20108",
            ToName: "Khách Mua",
            ToPhone: "0911111111",
            ToAddress: "456 Nhà Riêng",
            ToWardCode: "20101",
            ToDistrictId: 1530,
            CodAmount: 150000,
            WeightGram: 400,
            RequiredNote: "KHONGCHOXEMHANG",
            Items: [new ShippingItemModel("Áo", "SKU1", 1, 400)]);

        // Act
        var result = await adapter.CreateShipmentAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TrackingCode.Should().Be("GHN12345678");
        result.Value.TotalFee.Should().Be(32000);
    }

    [Fact]
    public async Task GetShippingLabelAsync_WhenValidTrackingCode_ShouldReturnPdfBytes()
    {
        // Arrange
        var tokenResponse = new GhnApiResponse<GhnGenTokenData>
        {
            Code = 200,
            Message = "Success",
            Data = new GhnGenTokenData { Token = "token_print_a5" }
        };

        var pdfData = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF header bytes

        var handler = new StubHandler((req, ct) =>
        {
            if (req.RequestUri!.AbsolutePath.Contains("gen-token"))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdfData)
            });
        });

        var client = new HttpClient(handler) { BaseAddress = new Uri(_options.BaseUrl) };
        var adapter = new GhnShippingAdapter(client, Options.Create(_options), NullLogger<GhnShippingAdapter>.Instance);

        // Act
        var result = await adapter.GetShippingLabelAsync("GHN12345678");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().Equal(pdfData);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => send(request, cancellationToken);
    }
}
