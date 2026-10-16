using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Orders;
using NovaLive.Api.Controllers.Shipping;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;
using NovaLive.Application.UseCases.Shipping.Queries.GetOrderTracking;
using NovaLive.Application.UseCases.Shipping.Queries.GetShippingLabel;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Tests.Shipping;

public sealed class SellerShippingControllerTests
{
    [Fact]
    public async Task Label_ReturnsPdfFileWithoutJsonEnvelope()
    {
        var id = Guid.NewGuid(); var bytes = "%PDF-1.7"u8.ToArray();
        var sender = new Mock<ISender>(MockBehavior.Strict);
        sender.Setup(s => s.Send(new GetShippingLabelQuery(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);
        var result = (FileContentResult)await new SellerShippingController(sender.Object).GetLabel(id, default);
        result.ContentType.Should().Be("application/pdf"); result.FileContents.Should().Equal(bytes);
        result.FileDownloadName.Should().Be($"shipping-label-{id}.pdf"); sender.VerifyAll();
    }

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    public async Task Label_MapsFailureThroughRespond(int status)
    {
        var id = Guid.NewGuid();
        Error error = status switch { 401 => ShippingErrors.Unauthorized,
            403 => Error.Forbidden("Shipping.Forbidden", "Denied"), _ => ShippingErrors.ShipmentNotFound };
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<GetShippingLabelQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[]>.Failure(error));
        ((ObjectResult)await new SellerShippingController(sender.Object).GetLabel(id, default)).StatusCode.Should().Be(status);
    }

    [Fact]
    public async Task CreationAndBothTrackingActions_DispatchCorrectRequestAndScope()
    {
        var sender = new Mock<ISender>(MockBehavior.Strict); var id = Guid.NewGuid();
        var request = new CreateShippingOrderRequest(id, "GHN", "2", Guid.NewGuid());
        sender.Setup(s => s.Send(new CreateShipmentOrderCommand(request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingOrderResponse(Guid.NewGuid(), id, "GHN", "TRACK", 20, "ReadyToPick", DateTime.UtcNow));
        sender.Setup(s => s.Send(new GetOrderTrackingQuery(id, true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<TrackingResponse>>.Success([]));
        sender.Setup(s => s.Send(new GetOrderTrackingQuery(id, false), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<TrackingResponse>>.Success([]));
        var seller = new SellerShippingController(sender.Object);
        ((ObjectResult)await seller.CreateOrder(request, default)).StatusCode.Should().Be(201);
        ((ObjectResult)await seller.GetTracking(id, default)).StatusCode.Should().Be(200);
        ((ObjectResult)await new OrdersController(sender.Object).GetTracking(id, default)).StatusCode.Should().Be(200);
        sender.VerifyAll();
    }

    [Fact]
    public void SellerController_RequiresAuthorizationAndCanonicalRoutes()
    {
        var type = typeof(SellerShippingController);
        type.GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().ContainSingle();
        type.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Should().BeEmpty();
        ((RouteAttribute)type.GetCustomAttributes(typeof(RouteAttribute), true).Single())
            .Template.Should().Be("api/v1/seller/shipping");
    }
}
