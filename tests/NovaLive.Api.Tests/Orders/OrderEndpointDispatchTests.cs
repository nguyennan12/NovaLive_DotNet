using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Orders;
using NovaLive.Application.UseCases.Orders.Commands.Checkout;
using NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
using NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
using NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
using NovaLive.Application.UseCases.Orders.Queries.GetMyOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetOrderDetail;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrderDetail;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
namespace NovaLive.Api.Tests.Orders;
public sealed class OrderEndpointDispatchTests
{
    [Fact]
    public void Controllers_RequireAuthenticationAndCanonicalRoutes()
    {
        foreach (var (type, route) in new[] { (typeof(OrdersController), "api/v1/orders"), (typeof(SellerOrdersController), "api/v1/seller/orders") })
        {
            type.GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().ContainSingle();
            ((RouteAttribute)type.GetCustomAttributes(typeof(RouteAttribute), true).Single()).Template.Should().Be(route);
        }
    }
    [Fact]
    public async Task Checkout_DispatchesHeaderAndReturnsCreated()
    {
        var request = new CheckoutRequest([Guid.NewGuid()], Guid.NewGuid(), [], null, null, null, "MoMo", null);
        var expected = new CheckoutResponse(Guid.NewGuid(), "NOVA-20261015-AB12", 100, "MoMo", Guid.NewGuid(), "Pending", [], null);
        var sender = new Mock<ISender>(MockBehavior.Strict);
        sender.Setup(s => s.Send(It.Is<CheckoutCommand>(c => c.Request == request && c.IdempotencyKey == "key"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expected));
        var response = (ObjectResult)await new OrdersController(sender.Object).Checkout(request, "key", default);
        response.StatusCode.Should().Be(201); ((ApiResponse<CheckoutResponse>)response.Value!).Data.Should().Be(expected);
        sender.VerifyAll();
    }
    [Theory]
    [InlineData(ErrorType.None, 200)]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.AlreadyExists, 409)]
    [InlineData(ErrorType.Unexpected, 500)]
    public async Task Commands_DispatchAndMapResult(ErrorType error, int status)
    {
        var id = Guid.NewGuid(); var request = new CancelOrderRequest("Reason"); var warehouse = new ConfirmSubOrderRequest(Guid.NewGuid());
        Result<OrderOperationResponse> result = error == ErrorType.None ? new OrderOperationResponse(id, "Cancelled") : new Error(error, "Test.OrderError");
        var sender = new Mock<ISender>(MockBehavior.Strict);
        sender.Setup(s => s.Send(It.Is<CancelOrderCommand>(q => q.OrderId == id && q.Request == request), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        sender.Setup(s => s.Send(It.Is<CancelSubOrderCommand>(q => q.SubOrderId == id && q.Request == request), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        sender.Setup(s => s.Send(It.Is<ConfirmSubOrderCommand>(q => q.SubOrderId == id && q.Request == warehouse), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        ((ObjectResult)await new OrdersController(sender.Object).Cancel(id, request, default)).StatusCode.Should().Be(status);
        var seller = new SellerOrdersController(sender.Object);
        ((ObjectResult)await seller.Cancel(id, request, default)).StatusCode.Should().Be(status);
        ((ObjectResult)await seller.Confirm(id, warehouse, default)).StatusCode.Should().Be(status);
        sender.VerifyAll();
    }
    [Fact]
    public async Task Queries_DispatchFiltersAndHideMissingDetails()
    {
        var id = Guid.NewGuid(); var from = DateTimeOffset.UtcNow.AddDays(-1); var to = from.AddDays(1);
        var sender = new Mock<ISender>(MockBehavior.Strict);
        sender.Setup(s => s.Send(new GetMyOrdersQuery("Pending", from, to, 2, 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PagedResult<ParentOrderListItemResponse>([], 2, 5, 0)));
        sender.Setup(s => s.Send(new GetSellerOrdersQuery("Confirmed", from, to, 2, 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PagedResult<SubOrderListItemResponse>([], 2, 5, 0)));
        sender.Setup(s => s.Send(new GetOrderDetailQuery(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ParentOrderDetailResponse>(Error.NotFound("Orders.OrderNotFound")));
        sender.Setup(s => s.Send(new GetSellerOrderDetailQuery(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SubOrderDetailResponse>(Error.NotFound("Orders.SubOrderNotFound")));
        var buyer = new OrdersController(sender.Object); var seller = new SellerOrdersController(sender.Object);
        ((ObjectResult)await buyer.GetOrders("Pending", from, to, 2, 5)).StatusCode.Should().Be(200);
        ((ObjectResult)await seller.GetOrders("Confirmed", from, to, 2, 5)).StatusCode.Should().Be(200);
        ((ObjectResult)await buyer.GetDetail(id, default)).StatusCode.Should().Be(404);
        ((ObjectResult)await seller.GetDetail(id, default)).StatusCode.Should().Be(404);
        sender.VerifyAll();
    }
}
