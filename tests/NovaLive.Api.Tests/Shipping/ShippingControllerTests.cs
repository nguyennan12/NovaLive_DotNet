using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Application.UseCases.Shipping.Queries.CalculateFee;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Shipping.Ghn;
using Xunit;

namespace NovaLive.Api.Tests.Shipping;

public class ShippingControllerTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly Mock<IGhnChecksumValidator> _validatorMock = new();
    private readonly ShippingController _controller;

    public ShippingControllerTests()
    {
        _controller = new ShippingController(_senderMock.Object, _validatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task GhnWebhook_WhenChecksumFails_ReturnsUnauthorized()
    {
        // Arrange
        var request = new GhnWebhookRequest("GHN123", "delivering");
        _validatorMock
            .Setup(v => v.ValidateWebhook(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _controller.GhnWebhook(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GhnWebhook_WhenChecksumSucceeds_DispatchesCommandAndReturnsOk()
    {
        // Arrange
        var request = new GhnWebhookRequest("GHN123", "delivered");
        _validatorMock
            .Setup(v => v.ValidateWebhook(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _senderMock
            .Setup(s => s.Send(It.IsAny<HandleShippingWebhookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _controller.GhnWebhook(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        _senderMock.Verify(s => s.Send(It.Is<HandleShippingWebhookCommand>(c => c.Request.OrderCode == "GHN123"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CalculateFee_WhenValid_ReturnsOkWithResponse()
    {
        // Arrange
        var request = new CalculateShippingFeeRequest(
            ShopId: Guid.NewGuid(),
            WarehouseAddressId: Guid.NewGuid(),
            ShippingAddressId: Guid.NewGuid(),
            WeightGram: 500);

        var feeResponse = new ShippingFeeResponse(
            Provider: "GHN",
            ServiceName: "Giao hàng chuẩn GHN",
            ShippingFee: 28000,
            EstimatedDeliveryDate: DateTime.UtcNow.AddDays(3));

        _senderMock
            .Setup(s => s.Send(It.IsAny<CalculateShippingFeeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(feeResponse));

        // Act
        var result = await _controller.CalculateFee(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeAssignableTo<ApiResponse<ShippingFeeResponse>>().Subject;
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.ShippingFee.Should().Be(28000);
        apiResponse.Data.Provider.Should().Be("GHN");

    }
}
