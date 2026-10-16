using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Orders;
using NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Tests.Orders;

public sealed class OrdersControllerTests
{
    [Fact]
    public void CalculateCheckout_RequiresAuthorizationAndCanonicalRoute()
    {
        var type = typeof(OrdersController);
        ((RouteAttribute)type.GetCustomAttributes(typeof(RouteAttribute), true).Single()).Template.Should().Be("api/v1/orders");
        var action = type.GetMethod(nameof(OrdersController.CalculateCheckout))!;
        action.GetCustomAttributes(typeof(AuthorizeAttribute), true).Should().ContainSingle();
        action.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Should().BeEmpty();
        ((HttpPostAttribute)action.GetCustomAttributes(typeof(HttpPostAttribute), true).Single()).Template.Should().Be("calculate-checkout");
    }

    [Theory]
    [InlineData(ErrorType.None, 200)]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Unauthorized, 401)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Unexpected, 500)]
    public async Task CalculateCheckout_DispatchesRequestWithCancellation_AndMapsResult(ErrorType errorType, int status)
    {
        var request = new CalculateCheckoutRequest([Guid.NewGuid()], Guid.NewGuid(), null, null, null);
        var preview = new CheckoutPreviewResponse(200m, 30m, 0m, 0m, 0m, 230m, []);
        Result<CheckoutPreviewResponse> result = errorType == ErrorType.None
            ? preview : new Error(errorType, "Checkout.Test", "Test failure");
        using var source = new CancellationTokenSource(); var ct = source.Token;
        var sender = new Mock<ISender>(MockBehavior.Strict);
        sender.Setup(x => x.Send(It.Is<CalculateCheckoutDraftQuery>(q => q.Request == request), ct)).ReturnsAsync(result);
        var response = (ObjectResult)await new OrdersController(sender.Object).CalculateCheckout(request, ct);
        response.StatusCode.Should().Be(status);
        var body = (ApiResponse<CheckoutPreviewResponse>)response.Value!;
        body.Success.Should().Be(errorType == ErrorType.None);
        if (errorType == ErrorType.None) body.Data.Should().Be(preview);
        else body.Error!.Code.Should().Be("Checkout.Test");
        sender.VerifyAll();
    }
}
